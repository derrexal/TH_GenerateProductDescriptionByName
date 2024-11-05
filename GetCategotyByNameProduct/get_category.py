import numpy as np
import pandas as pd
import gensim.downloader as api
import nltk
from nltk.tokenize import word_tokenize 
from nltk.corpus import stopwords
import re
import pymystem3
import pymorphy3


import logging
logging.basicConfig(filename= "get_category_logs.log", level=logging.DEBUG)
nltk.download('stopwords')
nltk.download('punkt')

stop_words = stopwords.words('russian')
mystem = pymystem3.Mystem()
morph = pymorphy3.MorphAnalyzer(lang='ru')
word2vec_model = api.load("word2vec-ruscorpora-300")

unique_categories_vectors_df = pd.read_pickle("unique_categories_vectors_df.pickle")

def clean_text(text: str) -> str:
    '''Clears text from extra symbols: *, +, urls, ., etc'''
    text = text.lower()
    regular = r'[\*+\#+\№\"\-+\+\=+\?+\&\^\.+\;\,+\>+\(\)\/+\:\\+]'
    regular_url = r'(http\S+)|(www\S+)|([\w\d]+www\S+)|([\w\d]+http\S+)'
    text = re.sub(regular, '', text)
    text = re.sub(regular_url, r'URL', text)
    text = re.sub(r'(\d+\s\d+)|(\d+)',' NUM ', text)
    text = re.sub(r'\s+', ' ', text)
    return text

def custom_tokenize(text: str) -> list[str]:
    '''Returns the list of words from the sentences'''
    if not text:
        print('The text to be tokenized is a None type. Defaulting to blank string.')
        text = ''
    return word_tokenize(text)

def poser(tokens: list[str]) -> list[str]:
    '''Returns the list of words + "_{morphological role}'''
    res = []
    for word in tokens:
        if word:
            res.append(word + '_' + str(morph.parse(word)[0].tag.POS))
        else:
            res.append(word)
    return res

def vec(tokens: list[str]) -> np.array:
    '''Returns numpy vector ready to count cosine similarity from tokens list'''
    res = np.array([0] * 300).astype('float64')
    c = 0
    for word in tokens:
        if word[-1] == 'F':
            if word[:-1] in word2vec_model.key_to_index:
                res += word2vec_model[word[:-1]]
                c += 1
            else:
                pass
        else:
            if word in word2vec_model.key_to_index:
                res += word2vec_model[word]
                c += 1
            else:
                pass
    return res / c if c != 0 else res


def get_vectors_from_raw_text(text: str) -> list[float]:
    """Summarizes preprocessing from raw text to vector"""
    text_cleaned = clean_text(text)
    text_tokenized = custom_tokenize(text_cleaned)
    text_no_stopwords = [word for word in text_tokenized if word not in stop_words]
    text_lemmatized = [mystem.lemmatize(word)[0] for word in text_no_stopwords]
    text_morph_analysed = poser(text_lemmatized)
    text_vectorized = vec(text_morph_analysed)
    return text_vectorized


def get_top_categories(text_input: str, N_top: int = 3) -> list[str]:
    """Returns TOP categories predicted by word2vec cosine similarity comparsion"""
    vector_input = get_vectors_from_raw_text(text_input)
    category_vectors = np.stack(unique_categories_vectors_df["Вектор Наименования конечной категории Портала"].values)
    cosine_similarities = word2vec_model.cosine_similarities(vector_input, category_vectors)
    
    idx_sorted = np.argsort(cosine_similarities)
    res = unique_categories_vectors_df.iloc[idx_sorted]
    res["cos_sim"] = np.sort(cosine_similarities)
    res_reverse = res.iloc[::-1].dropna()
    list_best_categories = res_reverse["Уникальное наименование конечной категории Портала"].tolist()[:N_top]

    return list_best_categories






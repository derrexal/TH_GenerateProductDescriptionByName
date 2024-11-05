using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net;

namespace HN_GenerateProductDescriptionByName.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchResultController : ControllerBase
    {
        private readonly int _limitSource = 10;
        private readonly string _searchEngine = "google";
        private string _baseUrl { get; set; }
        //"GET http://127.0.0.1:7000/google/search?text=Гугл&lang=RU&limit=5"

        private readonly string _trueCategoryUrl = "http://127.0.0.1:5000/api/getCategoryListFromNameProduct";
        private readonly ILogger<SearchResultController> _logger;
        private readonly CustomDbContext _context;
        
        public SearchResultController(ILogger<SearchResultController> logger, CustomDbContext context)
        {
            _logger = logger;
            _context = context;
            _baseUrl = $"http://127.0.0.1:7000/{_searchEngine}/search";
        }

        [HttpGet("GetProductDetails")]
        public async Task<List<ProductDetails>?> GetProductDetails()
        {
            var res = _context.ProductDetails != null ?
                        await _context.ProductDetails.ToListAsync() :
                        null;
            return res;
        }

        [HttpGet("GetCategoryListFromNameProduct")]
        public async Task<IList<string>> GetCategoryListFromNameProduct(string query)
        {
            //Убираем лишние символы из запроса (наименования товара)
            query = ReplaceChartersQuery(query);

            string trueCategoriesFullUrl = _trueCategoryUrl + "?name=" + query;
            try
            {
                string trueCategoriesResponse = await GetHttp(trueCategoriesFullUrl);
                var trueCategorieshResults = JsonConvert.DeserializeObject<List<string>>(trueCategoriesResponse);
                if (trueCategorieshResults is null)
                    throw new Exception("Категория не определена");
                return trueCategorieshResults;
            }
            catch (JsonReaderException)
            {
                _logger.LogError($"Не удалось распарсить ответ в JSON");
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Неизвестная ошибка сервиса определения категории");
                throw;
            }
        }

        [HttpGet("GetSearchResult")]
        public async Task<Dictionary<string, string>?> GetSearchResult(string query)
        {
            //Пока условимся что юзер не выбирает категорию, и мы доверяем MLL
            var categoryList = await GetCategoryListFromNameProduct(query);
                var currentCategory = categoryList.FirstOrDefault();
            //Получаем все характеристкии "выбранной" категории
                //TODO: это может не принять фронт! Нужно будет ДТО
                //var productDetailsCurrentCategory = await _context.ProductDetails.Where(p => p.PortalCategoryName == currentCategory).Select(p => new {p.detailId,p.detailName}).ToListAsync();
            var productDetailsCurrentCategory = await _context.ProductDetails.Where(p => p.PortalCategoryName == currentCategory).ToListAsync();
            //Убираем лишние символы из запроса (наименования товара)
            query = ReplaceChartersQuery(query);
            // Формируем url к сервису который "гуглит" товар и отдает самые релевантные ссылки по нему
            string searchProductUrl = ConstructUri(query);

            try
            {
                ////Получаем ссылки с которых нужно спарсить данные
                string searchResponse = await GetHttp(searchProductUrl);
                if (searchResponse.Contains("Timeout"))
                {
                    searchResponse = await GetHttp(searchProductUrl);
                    if (searchResponse.Contains("Timeout"))
                        throw new Exception("Time-out error");
                }

                var searchResults = JsonConvert.DeserializeObject<List<SearchResult>>(searchResponse);
                if (searchResults is null)
                    throw new Exception("Не найдено ссылок на товар");

                List<Dictionary<string, string>> detailsAndValueListGlobal = new List<Dictionary<string, string>>();
    
                //В цикле проходим по найденным ссылкам и парсим с них данные
                foreach (var searchResult in searchResults)
                {
                    string parseUrl = $"http://127.0.0.1:5000/api/getInfoFromUrl?url={searchResult.Url}";
                    var parseResponse = await GetHttp(parseUrl);
                    // если ответ пустой - смотрим другую ссылку
                    if (parseResponse.Length < 10) //условное ограничение (если ответ пустой)
                        continue;

                    // Получаем имена характеристик которые есть в тексте
                    string getCurrentProductDetailsUrl = $"http://127.0.0.1:5000/api/getDetailsFromQuestion";
                    var currentProductDetailsData = new OtherClassFindCurrentProductDetails
                    {
                        ProductDetailsNames = productDetailsCurrentCategory.Select(p=>p.detailName).ToList(),
                        Context = parseResponse,
                    };
                    var currentProductDetails = await PostHttpList(getCurrentProductDetailsUrl, currentProductDetailsData);
                    if (currentProductDetails is null) continue;

                    // Задаем вопросу - получаем значения характеристик
                    Dictionary<string, string> detailsAndValue = new Dictionary<string, string>();
                    foreach (string category in currentProductDetails)
                    {
                        var data = new OtherClassFindProductDetails
                        {
                            ProductDetail = category,
                            Context = parseResponse
                        };
                        string answerUrl = "http://127.0.0.1:5000/api/getAnswerFromQuestion";
                        string answerResponse = await PostHttp(answerUrl, data);
                        if (answerResponse == "" || answerResponse == "\"\"")
                            continue;
                        detailsAndValue.TryAdd(category, answerResponse);
                    }

                    detailsAndValueListGlobal.Add(detailsAndValue);
                }
                //Ищем список с самым большим количеством элементов и отдаем его на фронт 
                var winNum = detailsAndValueListGlobal.Max(p => p.Count);
                var winList = detailsAndValueListGlobal.FirstOrDefault(p=>p.Count==winNum);
                return winList;

                //using var response = await httpClie5nt.GetAsync(searchUrl);
                //jsonResponse = await response.Content.ReadAsStringAsync();
                //var searchResults = JsonConvert.DeserializeObject<List<SearchResult>>(jsonResponse);
                //if (searchResults != null)
                //    foreach(var searchResult in searchResults)
                //    {
                //        //Парсим данные
                //        string uri = $"http://127.0.0.1:5000/api/getInfoFromUrl?url={searchResult.Url}";
                //        using var response2 = await httpClient.GetAsync(uri);
                //        jsonResponse = await response2.Content.ReadAsStringAsync();
                //        //если ответ не пустой
                //        if (jsonResponse != "")
                //        {
                //            Console.WriteLine(jsonResponse);
                //        }

                //    }

                //Получаем значения категорий
                //test
                //string uri4 = $"http://127.0.0.1:5000/api/getAnswerFromQuestion";
                //using var response4 = await httpClient.GetAsync(uri4);
                //jsonResponse = await response4.Content.ReadAsStringAsync();
                //if (jsonResponse != "")
                //{
                //    Console.WriteLine(jsonResponse);
                //}
                //return jsonResponse;

                //Получаем категории
                //string uri3 = $"http://127.0.0.1:5000/api/getCategotyListFromNameProduct?name={query}";
                //using var response3 = await httpClient.GetAsync(uri3);
                //jsonResponse = await response3.Content.ReadAsStringAsync(); 
                //if (jsonResponse != "")
                //{
                //    Console.WriteLine(jsonResponse);
                //}

                //return searchResults;
                //throw new HttpRequestException("Поисковый движок не вернул ответ");
            }

            catch (JsonReaderException)
            {
                _logger.LogError($"Не удалось распарсить ответ в JSON");
                throw;
            }
            
            catch (Exception e)
            {
                _logger.LogError($"Неизвестная ошибка сервиса \n{e}");
                throw;
            }

        }


        [HttpGet("GetSearchResultOld")]
        public async Task<string?> GetSearchResultOld(string query) 
        {
            //Пока условимся что юзер не выбирает категорию, и мы доверяем MLL
            var categoryList = await GetCategoryListFromNameProduct(query);
            var currentCategory = categoryList.FirstOrDefault();
            //Получаем все характеристкии "выбранной" категории
            //TODO: это может не принять фронт! Нужно будет ДТО
            //var productDetailsCurrentCategory = await _context.ProductDetails.Where(p => p.PortalCategoryName == currentCategory).Select(p => new {p.detailId,p.detailName}).ToListAsync();
            var productDetailsCurrentCategory = await _context.ProductDetails.Where(p => p.PortalCategoryName == currentCategory).ToListAsync();
            //Убираем лишние символы из запроса (наименования товара)
            query = ReplaceChartersQuery(query);
            // Формируем url к сервису который "гуглит" товар и отдает самые релевантные ссылки по нему
            string searchProductUrl = ConstructUri(query);
            
            try
            {
                ////Получаем ссылки с которых нужно спарсить данные
                string searchResponse = await GetHttp(searchProductUrl);
                var searchResults = JsonConvert.DeserializeObject<List<SearchResult>>(searchResponse);
                if (searchResults is null)
                    throw new Exception("Не найдено ссылок на товар");

                //Сколько совпадений по наименованию характеристик в каждом из источников
                //TODO: зачем, если все равно мы смотрим смотрим по характеристикам с даннными
                Dictionary<SearchResult,int> countsContains = new Dictionary<SearchResult, int>();
                List<Dictionary<string,string>> detailsAndValueListGlobal = new List<Dictionary<string,string>>();

                //В цикле проходим по найденным ссылкам и парсим с них данные
                foreach (var searchResult in searchResults)
                {
                    int countContainsNameDetails = 0;
                    List<ProductDetails> currentProductDetails = new List<ProductDetails>();

                    string parseUrl = $"http://127.0.0.1:5000/api/getInfoFromUrl?url={searchResult.Url}";
                    var parseResponse = await GetHttp(parseUrl);
                    //если ответ не пустой
                    if (parseResponse.Length<10) //условное ограничение (если ответ пустой)
                        continue;

                    // ТУТ ищем наибольшее количество сонтейнсов в данных, где больше тот и берем. Найденные сохраняем и потом по ним вытаскиваем значения
                    foreach(var category in productDetailsCurrentCategory)
                    {
                        if (parseResponse.Contains(category.PortalCategoryName))
                        {
                            countContainsNameDetails++;
                            currentProductDetails.Add(category);
                        }                            
                    }
                    countsContains.Add(searchResult, countContainsNameDetails);

                    //Цикл по всем найденным характеристикам в тексте
                    //TODO: тут должен был быть еще какой-то счетчик. Вспоминай
                    Dictionary<string, string> detailsAndValue = new Dictionary<string, string>();
                    foreach(var category in currentProductDetails)
                    {
                        // Получаем значения характеристик
                        var data = new OtherClassFindProductDetails
                        {
                            ProductDetail = parseResponse,
                            Context = category.detailName
                        };
                        string answerUrl = "http://127.0.0.1:5000/api/getAnswerFromQuestion";
                        string answerResponse = await PostHttp(answerUrl, data);
                        if (answerResponse == "" || answerResponse == "\"\"")
                            continue;
                        detailsAndValue.Add(category.detailName, answerResponse);
                    }
                    detailsAndValueListGlobal.Add(detailsAndValue);
                }
                //Ищем список с самым большим количество элементов
                //detailsAndValueListGlobal.Max(p => p.Count();
                return "";

                //using var response = await httpClient.GetAsync(searchUrl);
                //jsonResponse = await response.Content.ReadAsStringAsync();
                //var searchResults = JsonConvert.DeserializeObject<List<SearchResult>>(jsonResponse);
                //if (searchResults != null)
                //    foreach(var searchResult in searchResults)
                //    {
                //        //Парсим данные
                //        string uri = $"http://127.0.0.1:5000/api/getInfoFromUrl?url={searchResult.Url}";
                //        using var response2 = await httpClient.GetAsync(uri);
                //        jsonResponse = await response2.Content.ReadAsStringAsync();
                //        //если ответ не пустой
                //        if (jsonResponse != "")
                //        {
                //            Console.WriteLine(jsonResponse);
                //        }

                //    }

                //Получаем значения категорий
                //test
                //string uri4 = $"http://127.0.0.1:5000/api/getAnswerFromQuestion";
                //using var response4 = await httpClient.GetAsync(uri4);
                //jsonResponse = await response4.Content.ReadAsStringAsync();
                //if (jsonResponse != "")
                //{
                //    Console.WriteLine(jsonResponse);
                //}
                //return jsonResponse;

                //Получаем категории
                //string uri3 = $"http://127.0.0.1:5000/api/getCategotyListFromNameProduct?name={query}";
                //using var response3 = await httpClient.GetAsync(uri3);
                //jsonResponse = await response3.Content.ReadAsStringAsync(); 
                //if (jsonResponse != "")
                //{
                //    Console.WriteLine(jsonResponse);
                //}

                //return searchResults;
                //throw new HttpRequestException("Поисковый движок не вернул ответ");
            }

            catch (JsonReaderException)
            {
                _logger.LogError($"Не удалось распарсить ответ в JSON");
                throw;
            }
        }

        private async Task<string> GetHttp(string url)
        {
            HttpClient httpClient = new HttpClient();
            string jsonResponse = "";
            try
            {
                using var response = await httpClient.GetAsync(url);
                jsonResponse = await response.Content.ReadAsStringAsync();
                if (jsonResponse == "")
                {
                    Console.WriteLine($"Пустой ответ сервиса {url}");
                }
                return jsonResponse;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Не доступен сервис:{url}\n{e}");
                throw;
            }
            catch(Exception e)
            {
                _logger.LogError($"Неизвестная ошибка сервиса {url}\n{e}");
                throw;
            }
        }

        private async Task<string> PostHttp(string url, object? data)
        {
            using HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = JsonContent.Create(data);

            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Метод {url} завершился с кодом:{response.StatusCode}");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (jsonResponse == "")
                {
                    Console.WriteLine($"Пустой ответ сервиса {url}");
                }
                return jsonResponse;
            }
            catch { throw; }
        }

        private async Task<List<string>?> PostHttpList(string url, object? data)
        {
            try
            {
                var res = await PostHttp(url, data);
                var resList = JsonConvert.DeserializeObject<List<string>>(res);
                return resList;
            }
            catch { throw; }
        }

        //TODO: Каждый раз отправлять такой большой пакет данных глуповато...
        private async Task<string> PostHttpOld(string url, object? data)
        {
            HttpClient httpClient = new HttpClient();
            var byteContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            string jsonResponse = "";
            try
            {
                using var response = await httpClient.PostAsync(url, byteContent);
                jsonResponse = await response.Content.ReadAsStringAsync();
                if (jsonResponse == "")
                {
                    Console.WriteLine($"Пустой ответ сервиса {url}");
                }
                return jsonResponse;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Не доступен сервис:{url}\n{e}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Неизвестная ошибка сервиса {url}\n{e}");
                throw;
            }
        }

        // Убираем лишние символы из наименования ТОВАРА
        private string ReplaceChartersQuery(string query)
        {
            var charsToRemove = new string[] { "@", ";", "'", "\"", "+", "-", "*", "|" };
            foreach (var c in charsToRemove)
                query = query.Replace(c, string.Empty);
            return query;

        }

        private string ConstructUri(string query)
        {
            string query1 = query;
            //string query2 = "~купить " + query + " характеристики описание";
            //string query3 = "~купить " + query;
            //string query4 = query + " характеристики";
            //string query5 = query + " описание";
            string query6 = "~" + query + " +характеристики +описание";
            //string query7 = query + " характеристики описание";

            //var totalQuery = $"{_baseUrl}?text={query1}|{query2}|{query3}|{query4}|{query5}|{query6}|{query7}&lang=RU&limit={_limitSource}";
            var totalQuery = $"{_baseUrl}?text={query1}|{query6}&lang=RU&limit={_limitSource}";

            return totalQuery;
        }
    }
}

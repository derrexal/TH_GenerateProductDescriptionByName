import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

// Асинхронный thunk для загрузки данных
export const fetchDataAsync = createAsyncThunk(
    'data/fetchData',
    async (data, { rejectWithValue }) => {
        try {

            const response = await fetch('SearchResult/GetSearchResult', {
                method: 'GET', // или 'GET', в зависимости от вашего API
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data),
            });

            if (!response.ok) {
                throw new Error('Ошибка запроса');
            }

            const responseData = await response.json();
            return responseData;
        } catch (error) {
            return rejectWithValue(error.message);
        }
    }
);

const initialState = {
    items: [],
    status: 'idle', // 'idle', 'loading', 'succeeded', 'failed'
    error: null,
    selection: {} // Состояние выбранного элемента из модального окна
};

export const dataSlice = createSlice({
    name: 'data',
    initialState,
    reducers: {
        // Reducer для обновления выбранного состояния из модального окна
        updateSelection: (state, action) => {
            state.selection = action.payload;
        },
    },
    extraReducers: (builder) => {
        builder
            .addCase(fetchDataAsync.pending, (state) => {
                state.status = 'loading';
            })
            .addCase(fetchDataAsync.fulfilled, (state, action) => {
                state.status = 'succeeded';
                state.items = action.payload; // Обновление данных, полученных от API
            })
            .addCase(fetchDataAsync.rejected, (state, action) => {
                state.status = 'failed';
                state.error = action.payload;
            });
    },
});

export const { updateSelection } = dataSlice.actions;

export default dataSlice.reducer;

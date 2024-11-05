import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';

function NotFoundPage() {
    const navigate = useNavigate();

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: '100vh',
            }}
        >
            <Typography variant="h4" component="h1" gutterBottom>
                Страница не найдена
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
                К сожалению, мы не можем найти страницу, которую вы ищете.
            </Typography>
            <Button variant="contained" onClick={() => navigate('/')}>
                Вернуться на главную
            </Button>
        </Box>
    );
}

export default NotFoundPage;

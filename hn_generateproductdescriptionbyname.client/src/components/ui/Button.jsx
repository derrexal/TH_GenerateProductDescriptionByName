import React from 'react';
import { Button as MuiButton } from '@mui/material';

const Button = ({ children, onClick, ...props }) => {
    return (
        <MuiButton
            variant="contained"
            color="primary"
            onClick={onClick}
            {...props}
        >
            {children}
        </MuiButton>
    );
}

export default Button;

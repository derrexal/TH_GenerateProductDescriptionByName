import React from 'react';
import { TextField } from '@mui/material';

const Input = ({ label, ...props }) => {
    return (
        <TextField
            variant="outlined"
            label={label}
            {...props}
        />
    );
}

export default Input;

import React, { useState, useEffect } from 'react';
import { Box, Modal, Button, Checkbox, FormControlLabel, FormGroup, Select, MenuItem, Typography, MobileStepper, IconButton, Radio, RadioGroup } from '@mui/material';
import KeyboardArrowLeft from '@mui/icons-material/KeyboardArrowLeft';
import KeyboardArrowRight from '@mui/icons-material/KeyboardArrowRight';

function ModalComponent({ open, handleClose, onApply, data, selectedCategory, closestCategories }) {
    const [page, setPage] = useState(0);
    const [checkboxState, setCheckboxState] = useState({});
    const [selectedOption, setSelectedOption] = useState('');
    const [radioValue, setRadioValue] = useState('');

    useEffect(() => {
        if (data) {
            resetStates(); // Call reset function on modal open
        }
    }, [data, page]); // Include page in dependency array to reset states on page change

    const resetStates = () => {
        const initialState = {};
        data.categories.forEach(category => {
            initialState[category.value] = false;
        });
        setCheckboxState(initialState);
        setSelectedOption('');
        setRadioValue('');
    };

    const handleCheckboxChange = (value) => {
        setCheckboxState(prev => ({ ...prev, [value]: !prev[value] }));
    };

    const handleApply = () => {
        let applyData;
        switch (page) {
            case 0: // Checkbox page
                const selectedCategories = Object.keys(checkboxState).filter(key => checkboxState[key]);
                applyData = { mainCategory: selectedCategories.length > 0 ? selectedCategories[0] : selectedCategory };
                break;
            case 1: // Radio buttons page
                applyData = { secondaryCategory: radioValue };
                break;
            case 2: // Select dropdown page
                applyData = { tertiaryCategory: selectedOption };
                break;
            default:
                applyData = {};
        }
        onApply(applyData);
        handleClose();
    };

    // Check if any value is selected on any page
    const isAnyValueSelected = () => {
        if (page === 0) { // For checkbox page
            return Object.values(checkboxState).some(v => v === true);
        } else if (page === 1) { // For radio buttons page
            return radioValue !== '';
        } else if (page === 2) { // For select dropdown page
            return selectedOption !== '';
        }
        return false;
    };

    const pages = [
        data && (
            <FormGroup>
                    <FormControlLabel
                        key={selectedCategory.key}
                        control={<Checkbox checked={checkboxState[selectedCategory.value] || false} onChange={() => handleCheckboxChange(selectedCategory.value)} name={selectedCategory.value} />}
                        // label={category.value}
                        label={selectedCategory.value}
                    />
            </FormGroup>
        ),
        data && (
            <RadioGroup value={radioValue} onChange={(event) => setRadioValue(event.target.value)}>
                {closestCategories.map(category => (
                    <FormControlLabel
                        key={category.value}
                        value={category.value}
                        control={<Radio />}
                        label={category.value}
                    />
                ))}
            </RadioGroup>
        ),
        data && (
            // Update this part to display all categories
            <Select fullWidth value={selectedOption} onChange={(e) => setSelectedOption(e.target.value)} displayEmpty>
                <MenuItem value=""><em>Выберите опцию</em></MenuItem>
                {data.categories.map(category => (
                    <MenuItem key={category.value} value={category.value}>{category.value}</MenuItem>
                ))}
            </Select>
        ),
    ];

    const maxPages = pages.filter(Boolean).length;

    return (
        <Modal open={open} onClose={handleClose}>
            <Box sx={{
                position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%, -50%)',
                bgcolor: 'background.paper', boxShadow: 24, p: 4,
                width: 500,
                height: 600,
                display: 'flex', flexDirection: 'column',
            }}>
                <Typography variant="h6" sx={{ mb: 2 }}>Вы можете выбрать категорию и нажать Применить</Typography>
                {pages[page]}
                <Typography variant="h6" sx={{ mb: 2 }}>Если среди предложенных вариантов нет вашей категории, продолжите выбор на другой странице</Typography>
                <MobileStepper
                    variant="dots"
                    steps={maxPages}
                    position="static"
                    activeStep={page}
                    nextButton={
                        <IconButton size="small" onClick={() => setPage(prevPage => Math.min(prevPage + 1, maxPages - 1))} disabled={page === maxPages - 1}>
                            <KeyboardArrowRight />
                        </IconButton>
                    }
                    backButton={
                        <IconButton size="small" onClick={() => setPage(prevPage => Math.max(prevPage - 1, 0))} disabled={page === 0}>
                            <KeyboardArrowLeft />
                        </IconButton>
                    }
                    sx={{ flexGrow: 1 }}
                />
                <Button onClick={handleApply} disabled={!isAnyValueSelected()} sx={{ mt: 2 }}>
                    Применить
                </Button>
            </Box>
        </Modal>
    );
}

export default ModalComponent;

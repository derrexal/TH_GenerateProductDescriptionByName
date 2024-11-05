// src/utils/validator.js

/**
 * Проверяет, не пустая ли строка.
 * @param {string} text - Строка для проверки.
 * @returns {boolean} - Возвращает true, если строка не пустая, иначе false.
 */
export const validateNotEmptyAndLength = (value) => {
    if (value.trim().length === 0) {
        return { isValid: false, message: "Это поле не может быть пустым" };
    }
    if (value.trim().length > 50) {
        return { isValid: false, message: "Превышено максимальное количество - 50 символов" };
    }
    return { isValid: true, message: "" };
};
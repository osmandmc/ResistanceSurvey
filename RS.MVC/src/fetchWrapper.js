// utils/fetchWrapper.js
export function fetchWithToken(url, options = {}) {
    const token = window.localStorage.getItem('token'); // Retrieve the token from localStorage

    const headers = {
        'Authorization': `Bearer ${token}`, // Add the token to the header
        'Content-Type': 'application/json',
        ...options.headers, // Merge with any additional headers passed in options
    };

    return fetch(url, {
        ...options,
        headers,
    });
}

import { createApp } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';
import ResistanceOverview from './components/ResistanceOverview.vue';
import router from './router.js'; // Import your routes from a separate file

import $ from 'jquery'; // Import jQuery globally


// Create the Vue app
const app = createApp(ResistanceOverview);

window.$ = window.jQuery = $;

// Use the router in the app
app.use(router);

// Mount the app to the DOM
app.mount('#resistance');

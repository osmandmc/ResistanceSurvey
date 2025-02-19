import { createApp } from 'vue';
import ResistanceOverview from './components/ResistanceOverview.vue';
import router from './router.js'; // Import your routes from a separate file
import VueDatePicker from '@vuepic/vue-datepicker';

import $ from 'jquery'; // Import jQuery globally

import 'sweetalert2/dist/sweetalert2.min.css';
import '@vuepic/vue-datepicker/dist/main.css'

import VueSweetalert2 from 'vue-sweetalert2';

// Create the Vue app
const app = createApp(ResistanceOverview);

window.$ = window.jQuery = $;

// Use the router in the app
app.use(VueSweetalert2);
app.component('VueDatePicker', VueDatePicker);
app.use(router);

// Mount the app to the DOM
app.mount('#resistance');

<template>
  <div class="ui form">
    <Protesto 
        :protesto="protesto"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
    />
    <!-- Save Button -->
    <button
        class="ui primary button"
        type="button"
        @click="saveProtesto"
    >
      EYLEMİ KAYDET
    </button>
    <button
        class="ui primary button"
        type="button"
        @click="cancelProtesto"
    >
      VAZGEÇ
    </button>
    <button 
        v-if="protesto.protestoId !== null"
        class="ui primary button"
        type="button"
        @click="deleteProtesto"
    >
      SİL
    </button>
  </div>
</template>

<script>
import Multiselect from "vue-multiselect";
import VueDatePicker from '@vuepic/vue-datepicker';
import Protesto from "./Protesto.vue";
import '@vuepic/vue-datepicker/dist/main.css'
import {fetchWithToken} from "../../fetchWrapper";

export default {
  name: "EditProtesto",
  emits: ["cancelProtesto"],
  components: { Protesto },
  props: {
    protesto: {
      type: Object,
      required: true,
    },
    protestoTypeOptions: {
      type: Array,
      required: true,
    },
    protestoPlaceOptions: {
      type: Array,
      required: true,
    },
    genderOptions: {
      type: Array,
      required: true,
    },
    employeeCountInProtestoOptions: {
      type: Array,
      required: true,
    },
  },
  methods: {
    saveProtesto() {
      console.log(this.protesto);
      fetchWithToken("/Resistance/CreateOrUpdateProtesto", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(this.protesto)
      })
          .then(response => { 
            console.log(response);
            this.$router.push(`/edit/${this.protesto.resistanceId}`);
          })
          .catch(error => console.log(error));
      
    },
    deleteProtesto() {
      fetchWithToken(`/Resistance/DeleteProtesto/${this.protesto.protestoId}`, {
        method: 'DELETE',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        }
      })
          .then(response => {
            console.log(response);
            this.$router.push(`/edit/${this.protesto.resistanceId}`);
          })
          .catch(error => console.log(error));
    },
    cancelProtesto() {
      console.log(this.protesto);
      this.$emit('cancelProtesto', this.protesto);
    }
  },
};
</script>

<style scoped>
/* Add any scoped styles here if necessary */
</style>

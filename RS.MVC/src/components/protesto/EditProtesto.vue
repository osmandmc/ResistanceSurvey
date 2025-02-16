<template>
  <div v-if="this.isLoading" class="ui dimmer active">
    <div class="ui text loader">İşlem Yapılıyor...</div>
  </div>
  <div class="ui form">
    <Protesto 
        :protesto="this.protestoData"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        :cities="this.cities"
        :districts="this.districts"
        :intervention-types="this.interventionTypes"
        @addLocation="handleAddLocation"
        @deleteLocation="handleDeleteLocation"
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
import protesto from "./Protesto.vue";

export default {
  name: "EditProtesto",
  emits: ["cancelProtesto", 'onProtestoAdded', "onProtestoDeleted"],
  components: { Protesto },
  data() {
    return {
      isLoading: false,
      protestoData: {
        ...this.protesto,
      },
      formErrors: {},
    }
  },
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
    cities: {
      type: Array,
      required: true,
    },
    districts: {
      type: Array,
      required: true,
    },
    interventionTypes: {
      type: Array,
      required: true,
    }
  },
  methods: {
    saveProtesto() {
      // Save form logic
      const errors = this.validateProtesto();
      console.log(errors);
      // If there are errors, display them and stop submission
      if (Object.keys(errors).length > 0) {
        this.formErrors = errors; // Update formErrors to display validation messages
        return;
      }

      this.isLoading = true;
      console.log(this.protestoData);
      fetchWithToken("/ResistanceApi/CreateOrUpdateProtesto", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(this.protestoData)
      })
          .then(response => {
            this.isLoading = false;
            // Handle HTTP errors
            if (response.status === 400) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: response.statusText,
              });
              return Promise.reject(response.statusText); // Stop further processing
            } else if (response.status === 500) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: 'Yazilim destek: ' + response.statusText,
              });
              return Promise.reject(response.statusText); // Stop further processing
            }
            return response.json(); // Return the parsed JSON for the next .then()
          })
          .then(data => {
            console.log(data); // Log the parsed response data
            const id = data; // Assuming the response is the ID (integer)
            this.$swal('Eylem kaydedildi');
            this.$emit('onProtestoAdded', { protesto: this.protestoData, id });
          })
          .catch(error => {
            this.isLoading = false;
            this.$swal.fire({
              icon: "error",
              title: "Bir hata olustu",
            });
            console.log(error);
          });
  
    },
    deleteProtesto() {
      this.isLoading = true;
      fetchWithToken(`/ResistanceApi/DeleteProtesto/${this.protesto.protestoId}`, {
        method: 'DELETE',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        }
      })
          .then(response => {
            this.isLoading = false;
            // Handle HTTP errors
            if (response.status === 400) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: response.statusText,
              });
              return Promise.reject(response.statusText); // Stop further processing
            } else if (response.status === 500) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: 'Yazilim destek: ' + response.statusText,
              });
              return Promise.reject(response.statusText); // Stop further processing
            }
            this.$swal('Eylem silindi');
            this.$emit('onProtestoDeleted', this.protestoData.protestoId );
          })
          .catch(error => {
            this.isLoading = false;
            this.$swal.fire({
              icon: "error",
              title: "Bir hata olustu",
            });
            console.log(error);
          });
    },
    cancelProtesto() {
      this.$emit('cancelProtesto', this.protesto);
    },
    handleAddLocation(){
      this.protestoData.locations.push({id:0, protestoId: this.protesto.protestoId, cityId:null, districtId: null, place: null, deleted: false});
    },
    handleDeleteLocation(index) {
      const location = this.protestoData.locations[index];
      location.deleted = true;
    },
    validateProtesto() {
      const errors = {};

      // ProtestoTypeIds: At least one protest type selected
      if (!this.protestoData.protestoTypeIds || this.protestoData.protestoTypeIds.length === 0) {
        errors.protestoTypeIds = "Lütfen en az bir eylem türü seçiniz.";
      }

      // ProtestoPlaceIds: At least one protest place selected
      if (!this.protestoData.protestoPlaceIds || this.protestoData.protestoPlaceIds.length === 0) {
        errors.protestoPlaceIds = "Lütfen en az bir eylem mekanı seçiniz.";
      }

      // GenderId: Required
      if (!this.protestoData.genderId) {
        errors.genderId = "Lütfen bir cinsiyet giriniz.";
      }

      // ProtestoStartDate: Required
      if (!this.protestoData.protestoStartDate) {
        errors.protestoStartDate = "Lütfen başlangıç tarihi seçiniz.";
      }

      // EmployeeCountInProtesto: Required if EmployeeCountInProtestoId is not provided
      if (!this.protestoData.employeeCountInProtesto && !this.protestoData.employeeCountInProtestoId) {
        errors.employeeCountInProtesto = "Lütfen eylemdeki işçi sayısını giriniz.";
      }

      // InterventionTypeIds: At least one intervention type selected
      if (!this.protestoData.interventionTypeIds || this.protestoData.interventionTypeIds.length === 0) {
        errors.interventionTypeIds = "Lütfen en az bir müdahale tipi seçiniz.";
      }

      // CustodyCount: Required if custody is possible
      if (this.isCustodyPossible && !this.protestoData.custodyCount) {
        errors.custodyCount = "Lütfen gözaltı sayısını giriniz.";
      }

      return errors;
    },
  },
};
</script>

<style scoped>
/* Add any scoped styles here if necessary */
</style>

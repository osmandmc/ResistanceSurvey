<template>
  <div class="ui form">
    <Protesto 
        :protesto="this.protestoData"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        :cities="this.cities"
        :districts="this.districts"
        @addLocation="handleAddLocation"
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
  emits: ["cancelProtesto"],
  components: { Protesto },
  data() {
    return {
      protestoData: {
        locations: []
      }
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
    }
  },
  mounted() {
    console.log(protesto);
    this.protestoData = { ...this.protesto };
    console.log(this.cities);
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
      this.$emit('cancelProtesto', this.protesto);
    },
    
    handleAddLocation(){
      this.protestoData.locations.push({});
    }
  },
};
</script>

<style scoped>
/* Add any scoped styles here if necessary */
</style>

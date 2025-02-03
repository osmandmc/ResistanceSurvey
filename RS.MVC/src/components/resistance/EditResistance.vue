<template>
  <div v-if="isLoading" class="ui dimmer active">
    <div class="ui text loader">İşlem Yapılıyor...</div>
  </div>
  <h4 class="ui dividing header">
    <router-link to="/list" class="item"><i class="angle double left icon"></i></router-link>
    Vaka
  </h4>
  <form class="ui form">
    <input type="hidden" name="Id" v-model="resistance.id" />
    <Resistance 
        :resistance="resistance"
    >
    </Resistance>
   
    <!-- Protestos -->
    <h3 class="ui dividing header">Eylemler</h3>
    <button
        type="button"
        class="ui right labeled small green icon button"
        @click="addProtesto"
    >
      Eylem Ekle<i class="plus icon"></i>
    </button>
    <protesto-accordion
        :protestoItems="resistance.protestoItems"
        :protestoPlaceOptions="protestoPlaceOptions"
        :protestoTypeOptions="protestoTypeOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
    />

    <!-- Save and Cancel Buttons -->
    <button id="btnSave" class="ui primary button" @click.prevent="saveForm">
      KAYDET
    </button>
    <button type="button" id="btnCancelResistanceModal" class="ui negative button">
      SİL
    </button>
  </form>
  <company-modal ref="modalRef"
         :companyTypes="companyTypes"
         :companyScales="companyScales"
         :worklines="worklines"
         :companies="companies"
         @save-company="handleSaveCompany"/>

</template>

<script>
import {fetchWithToken} from "../../fetchWrapper";
import Resistance from "./Resistance.vue";
import Multiselect from 'vue-multiselect'
import ProtestoAccordion from "../protesto/ProtestoAccordion.vue";
import CompanyModal from "../CompanyModal.vue"; // Adjust the path based on your folder structure


export default {
  name: "EditResistance",
  components: {CompanyModal, ProtestoAccordion, Multiselect, Resistance},
  data() {
    return {
      isLoading: true,
      resistance: {},
      categories: [],
      resistanceReasons: [],
      companies: [],
      corporations: [],
      employeeCounts: [],
      protestoPlaceOptions: [], 
      protestoTypeOptions: [],
      genderOptions: [],
      employeeCountInProtestoOptions:[],
      companyTypes: [],
      companyScales: [],
      worklines: [],
      formErrors: {},
    };
  },
  props: ['id'],  // The id is passed as a prop from the router
  mounted() {
    this.fetchResistance();
    fetchWithToken("/company/list")
        .then(response => response.json())
        .then(data => (this.companies = data));

    fetchWithToken("/resistance/categories")
        .then(response => response.json())
        .then(data => (this.categories = data));

    fetchWithToken("/lookup/resistancereasons")
        .then(response => response.json())
        .then(data => (this.resistanceReasons = data));
    
  
    fetchWithToken("/corporation/list")
        .then(response => response.json())
        .then(data => (this.corporations = data));

    fetchWithToken("/lookup/companyTypes")
        .then(response => response.json())
        .then(data => (this.companyTypes = data));

    fetchWithToken("/lookup/companyScales")
        .then(response => response.json())
        .then(data => (this.companyScales = data));
    
    fetchWithToken("/lookup/companyWorklines")
        .then(response => response.json())
        .then(data => (this.worklines = data));

    fetchWithToken("/lookup/employeeCountInProtesto")
        .then(response => response.json())
        .then(data => (this.employeeCountInProtestoOptions = data));

    this.initializeSemanticUI();
  },
  updated() {
    this.initializeSemanticUI();
  },
  watch: {
    // Watch for changes in the 'id' prop and reload the data
    '$route.params.id': 'fetchResistance',
  },
  methods: {
    fetchResistance() {
      this.isLoading = true;
      const id = this.$route.params.id;  // Accessing the id directly from $route.params

      // Simulate an API call
      fetchWithToken(`/resistance/get/${id}`)
          .then(response => response.json())
          .then(data => { 
            console.log(data); 
            this.resistance = data; 
            this.isLoading = false; 
          });      
    },
    initializeSemanticUI() {
      // Reinitialize Semantic UI components
      this.$nextTick(() => {
        $('.ui.accordion').accordion();
      });
    },
    toggleOutsource() {
      // Show/hide outsource section
    },
    openCompanyModal(isMain) {
      this.$refs.modalRef.openModal();
    },
    handleSaveCompany(companyData) {
      console.log(companyData);
      fetchWithToken("/Company/Create/", { 
        method: 'POST',
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(companyData)})
          .then(response => response.json())
          .then(data => (console.log(data)));
      this.companies.push(companyData);
    },
    saveForm() {
      // Save form logic
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date
    },
    addResistanceReason (newTag) {
      const tag = {
        name: newTag,
        id: -1
      }
      console.log(tag);
      this.resistanceReasons.push(tag)
      this.resistance.resistanceReasonIds.push(tag)
    }
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>

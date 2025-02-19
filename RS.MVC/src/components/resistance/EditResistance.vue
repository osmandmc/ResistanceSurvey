<template>
  <div v-if="this.isLoading" class="ui dimmer active">
    <div class="ui text loader">İşlem Yapılıyor...</div>
  </div>
  <h4 class="ui dividing header">
    <router-link to="/list" class="item"><i class="angle double left icon"></i></router-link>
    Vaka
  </h4>
  <form class="ui form">
    <input type="hidden" name="Id" v-model="this.resistance.id" />
    <Resistance
        :resistance="this.resistance"
        :corporations="corporations"
        :resistanceReasons="resistanceReasons"
        :employeeCounts="employeeCounts"
        :companies="companies"
        :categories="categories"
        :tradeUnions="tradeUnions"
        :tradeUnionAuthorities="tradeUnionAuthorities"
        :employmentTypes="employmentTypes"
        :formErrors="formErrors"
        @openCompanyModal="handleOpenCompanyModal"
        @onInputChanged="clearFormError"
    />
   
    <!-- Protestos -->
    <h3 class="ui dividing header">Eylemler</h3>
    <button
        type="button"
        class="ui right labeled small green icon button"
        @click="createProtesto"
    >
      Eylem Ekle<i class="plus icon"></i>
    </button>
    <protesto-accordion
        :activeProtestoIndex="this.activeProtestoIndex"
        :protestoItems="resistance.protestoItems"
        :protestoPlaceOptions="protestoPlaceOptions"
        :protestoTypeOptions="protestoTypeOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        :cities="cities"
        :districts="districts"
        :intervention-types="interventionTypes"
        @cancelProtesto="handleCancelProtesto"
        @saveProtesto="handleSaveProtesto"
        @deleteProtesto="handleDeleteProtesto"
        @onChangeActiveProtesto="handleActiveProtesto"
        @onInputChanged="clearFormError"
    />
    <resistance-news :news="this.resistance.resistanceNews" @removeNews="handleRemoveNews"/>
    
    <!-- Save and Cancel Buttons -->
    <button v-if="showResistanceButton" class="ui primary button" @click.prevent="saveForm">
      KAYDET
    </button>
    <button v-if="showResistanceButton" class="ui negative button" type="button" @click="deleteForm">
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
import ResistanceNews from "../news/ResistanceNews.vue";
import CompanyModal from "../CompanyModal.vue";
import {inject, watch} from 'vue';

export default {
  name: "EditResistance",
  components: {CompanyModal, ProtestoAccordion, Multiselect, Resistance, ResistanceNews},
  emits: ["openCompanyModal"],
  setup() {
    const addedNews = inject('addedNews'); // Inject full object
    return { addedNews };
  },
  data() {
    return {
      isLoading: true,
      resistance: {
        id: null, // Default value for Id (could be null or -1 based on your use case)
        resistanceDescription: '', // Default empty string for description
        categoryId: '', // Empty string for the category ID
        resistanceReasonIds: [], // Default to an empty array for selected reasons
        developRight: '', // Default empty string (you might want a boolean or string value depending on your form logic)
        companyId: '', // Default to an empty string for the company ID
        isOutsource: false, // Default to 'false' indicating the company is not outsourced
        mainCompanyId: '', // Default empty string for main company, if outsourcing is selected
        employeeCountId: '', // Default to an empty string for employee count ID
        employeeCount: '', // Default empty string for employee count (could be a number if required)
        corporationIds: [], // Default empty array for selected corporation IDs
        resistanceResult: 0, // Default to "Bilinmiyor" (unknown result)
        protestoItems: [],
        resistanceNews: [],
      },
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
      cities: [],
      districts: [],
      tradeUnionAuthorities:[],
      tradeUnions: [],
      employmentTypes: [],
      interventionTypes:[],
      activeProtestoIndex: null,
      formErrors: {},
    };
  },
  mounted() {
    this.fetchResistance();
    fetchWithToken("/company/list")
        .then(response => response.json())
        .then(data => (this.companies = data));

    fetchWithToken("/lookup/categories")
        .then(response => response.json())
        .then(data => (this.categories = data));

    fetchWithToken("/lookup/resistancereasons")
        .then(response => response.json())
        .then(data => (this.resistanceReasons = data));

    fetchWithToken("/lookup/employeeCounts")
        .then(response => response.json())
        .then(data => (this.employeeCounts = data));

    fetchWithToken("/corporation/list")
        .then(response => response.json())
        .then(data => {
          this.corporations = data;
          this.tradeUnions = data.filter(s=>s.CorporationTypeId = 1)
        });
    
    fetchWithToken("/lookup/tradeUnionAuthorities")
        .then(response => response.json())
        .then(data => (this.tradeUnionAuthorities = data));

    fetchWithToken("/lookup/companyTypes")
        .then(response => response.json())
        .then(data => (this.companyTypes = data));

    fetchWithToken("/lookup/companyScales")
        .then(response => response.json())
        .then(data => (this.companyScales = data));

    fetchWithToken("/lookup/companyWorklines")
        .then(response => response.json())
        .then(data => (this.worklines = data));

    fetchWithToken("/lookup/EmployeeCountInProtesto")
        .then(response => response.json())
        .then(data => (this.employeeCountInProtestoOptions = data));

    fetchWithToken("/ProtestoPlace/List")
        .then(response => response.json())
        .then(data => (this.protestoPlaceOptions = data));

    fetchWithToken("/ProtestoType/List")
        .then(response => response.json())
        .then(data => (this.protestoTypeOptions = data));

    fetchWithToken("/lookup/genders")
        .then(response => response.json())
        .then(data => (this.genderOptions = data));

    fetchWithToken("/lookup/cities")
        .then(response => response.json())
        .then(data => (this.cities = data));

    fetchWithToken("/lookup/districts")
        .then(response => response.json())
        .then(data => (this.districts = data));
    
    fetchWithToken("/lookup/employmentTypes")
        .then(response => response.json())
        .then(data => (this.employmentTypes = data));
    fetchWithToken("/lookup/interventionTypes")
        .then(response => response.json())
        .then(data => (this.interventionTypes = data));
    // this.initializeSemanticUI();
  },
  watch: {
    '$route.params.id': 'fetchResistance',
    'addedNews.news': {
      handler(newNews) {
        console.log(newNews);
        if (newNews && this.resistance) {
          console.log(newNews);
          this.resistance.resistanceNews.push(newNews); // Push into array
        }
      },
      deep: true,
    },
  },
  methods: {
    fetchResistance() {
      this.isLoading = true;
      const id = this.$route.params.id;  // Accessing the id directly from $route.params

      // Simulate an API call
      fetchWithToken(`/resistanceapi/get/${id}`)
          .then(response => response.json())
          .then(data => { 
            this.resistance = data; 
            this.isLoading = false; 
          });      
    },
    createProtesto(){
      console.log('created');
      const created = this.resistance.protestoItems.filter(s=>s.protestoId === 0);
      if(created.length === 0){
        const protesto = {
          resistanceId: this.resistance.id,
          protestoId: 0,
          protestoTypeIds: [],
          isAgainstProduction: false,
          protestoStartDate: null,
          protestoEndDate: null,
          protestoPlaceIds: [],
          genderId: null,
          interventionTypeIds: [],
          protestoCityIds: [],
          protestoDistrictIds: [],
          locations: [],
          custodyCount: null,
          employeeCountInProtesto: null,
          employeeCountInProtestoId: null,
          resistanceName: "",
          note: "",
          simpleProtestoDescription: "",
          strikeDuration: 0
        }

        this.activeProtestoIndex = this.resistance.protestoItems.length;
        this.resistance.protestoItems.push(protesto);
        console.log(this.activeProtestoIndex);
      }
      else{
        this.activeProtestoIndex = this.resistance.protestoItems.length;
      }
    },
    toggleOutsource() {
      // Show/hide outsource section
    },
    handleOpenCompanyModal() {
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
          .then(response => {
            return response.json();
          })
          .then(data => {
            console.log(data);
            this.companies.push(data);
            this.resistance.companyId = data.id;
          });
     
    },
    saveForm() {
      // Save form logic
      console.log(this.resistance);
      const errors = this.validateForm();
      console.log(errors);
      // If there are errors, display them and stop submission
      if (Object.keys(errors).length > 0) {
        this.formErrors = errors; // Update formErrors to display validation messages
        return;
      }

      this.isLoading = true;
      fetchWithToken("/Resistanceapi/UpdateResitance", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(this.resistance)
      })
      .then(response => { 
        console.log(response);
        this.isLoading = false;
        this.$swal('Vaka kaydedildi');
        this.$router.push({ path: `/` });      
      })
      .catch(error =>
      {
        this.isLoading = false;
        this.$swal({
          icon: "error",
          title: "Bir hata olustu",
          text: "Bir hata olustu!",
        });
        console.log(error)
      });
    },
    deleteForm() {
      // Save form logic
      this.isLoading = true;
      fetchWithToken("/ResistanceApi/DeleteResistance/" + this.resistance.id, {
        method: 'DELETE',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
      })
          .then(response => {
            console.log(response);
            this.isLoading = false;
            this.$swal('Vaka silindi');
            this.$router.push({ path: `/` });
          })
          .catch(error =>
          {
            this.isLoading = false;
            this.$swal({
              icon: "error",
              title: "Bir hata olustu",
              text: "Bir hata olustu!",
            });
            console.log(error)
          });
    },
    handleCancelProtesto() {
      console.log(this.activeProtestoIndex, 'Cancel');
      this.resistance.protestoItems = this.resistance.protestoItems.filter(s=>s.protestoId !== 0);
      this.activeProtestoIndex = null;
    },
    handleSaveProtesto(obj) {
      const { id, protesto } = obj; // Destructure the object for easier access
      protesto.protestoId = id;
      // Check if the protestoId already exists in protestoItems
      const existingIndex = this.resistance.protestoItems.findIndex(item => item.protestoId === id);

      if (existingIndex !== -1) {
        console.log('update');
        // Update the existing protesto item
        this.resistance.protestoItems[existingIndex] = { ...this.resistance.protestoItems[existingIndex], ...protesto };
      } else {
        console.log('insert');
        const createdIndex = this.resistance.protestoItems.findIndex(item => item.protestoId === 0);
        this.resistance.protestoItems[createdIndex] = { ...this.resistance.protestoItems[existingIndex], ...protesto };
      }
      this.activeProtestoIndex = null;
      // Optionally, you can emit an event or perform other actions here
      console.log("Protesto saved:", this.resistance.protestoItems);
    },
    handleDeleteProtesto(id) {
      this.resistance.protestoItems = this.resistance.protestoItems.filter(s=>s.protestoId !== id);
      this.activeProtestoIndex = null;
    },
    handleActiveProtesto(activeIndex){
      this.activeProtestoIndex = activeIndex;
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date
    },
    addResistanceReason (newTag) {
      const tag = {
        name: newTag,
        id: -1
      }
      this.resistanceReasons.push(tag)
      this.resistance.resistanceReasonIds.push(tag)
    },
    handleRemoveNews(newsId){
      console.log('remove newsId ',newsId);
      this.resistance.resistanceNews = this.resistance.resistanceNews.filter(s=>s.id !== newsId);
    },
    validateForm() {
      const errors = {};

      if (!this.resistance.resistanceDescription) {
        errors.resistanceDescription = "Lütfen bir kısa açıklama seçiniz.";
      }
      // CategoryId: Required
      if (!this.resistance.categoryId) {
        errors.categoryId = "Lütfen bir kategori giriniz.";
      }

      // CompanyId: At least one company selected
      if (!this.resistance.companyId) {
        errors.companyId = "Lütfen bir şirket seçiniz.";
      }

      // CorporationIds: At least one corporation selected
      if (!this.resistance.corporationIds || this.resistance.corporationIds.length === 0) {
        errors.corporationIds = "Lütfen en az bir kurumsallık seçiniz.";
      }

      // EmploymentTypeIds: At least one employment type selected
      if (!this.resistance.employmentTypeIds || this.resistance.employmentTypeIds.length === 0) {
        errors.employmentTypeIds = "Lütfen en az bir istihdam türü seçiniz.";
      }

      // AnyLegalIntervention: Required
      if (this.resistance.anyLegalIntervention === null || this.resistance.anyLegalIntervention === undefined) {
        errors.anyLegalIntervention = "Hukuki girişim var mı?";
      }

      // DevelopRight: Required
      if (this.resistance.developRight === null || this.resistance.developRight === undefined) {
        errors.developRight = "Hak Geliştirmeye/Savunma Özelliği";
      }

      return errors;
    },
    clearFormError(field){
      this.formErrors[field] = "";
    }
  },
  computed: {
    showResistanceButton() {
      return this.activeProtestoIndex === null
    } 
  }
};
</script>

<style scoped>
/* Add styles here */
</style>

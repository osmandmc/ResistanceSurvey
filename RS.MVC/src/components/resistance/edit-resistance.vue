<template>
  <div v-if="isLoading" class="ui dimmer active">
    <div class="ui text loader">İşlem Yapılıyor...</div>
  </div>
  <form id="resistanceForm" class="ui form">
    <!-- Header -->
    <h4 class="ui dividing header">
      <router-link to="/list" class="item"><i class="angle double left icon"></i></router-link>
      Vaka
    </h4>

    <!-- Hidden Input for ID -->
    <input type="hidden" name="Id" v-model="resistance.id" />

    <!-- Short Description -->
    <div class="field">
      <label for="ResistanceDescription">Kısa Açıklama</label>
      <textarea
          id="ResistanceDescription"
          rows="6"
          v-model="resistance.resistanceDescription"
      ></textarea>
    </div>

    <!-- Case Category -->
    <div class="field">
      <label for="CategoryId">Vaka Niteliği</label>
      <select v-model="resistance.categoryId">
        <option value="">--Seçiniz--</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </select>
      <span v-if="formErrors.categoryId" class="text-danger">
        {{ formErrors.categoryId }}
      </span>
    </div>

    <!-- Two Fields -->
    <div class="two fields">
      <!-- Case Reasons -->
      <div class="field">
        <label for="ResistanceReasonIds">Vaka Nedeni</label>
        <multiselect id="ResistanceReasonIds" v-model="resistance.resistanceReasonIds" :options="resistanceReasons" 
                     :multiple="true" :close-on-select="false" :clear-on-select="false"
                     :preserve-search="true" placeholder="Seçiniz" label="name" track-by="id" :preselect-first="true"
                     :taggable="true" @tag="addTag">
         
        </multiselect>
      </div>

      <!-- Develop Right -->
      <div class="field">
        <label for="DevelopRight">Hak Geliştirme/Hak Savunma Özelliği</label>
        <select v-model="resistance.developRight">
          <option value="">--Seçiniz--</option>
          <option :value="true">Hak Geliştirme</option>
          <option :value="false">Hak Savunma</option>
        </select>
        <span v-if="formErrors.developRight" class="text-danger">
          {{ formErrors.developRight }}
        </span>
      </div>
    </div>

    <div class="sixty wide field">
      <label for="CompanyId">Şirket</label>
      <div class="two fields">
        <div class="field">
          <select v-model="resistance.companyId">
            <option value="">--Seçiniz--</option>
            <option
                v-for="company in companies"
                :key="company.id"
                :value="company.id"
            >
              {{ company.name }}
            </option>
          </select>
        </div>
        <div class="field">
          <button type="button" @click="openCompanyModal(true)" class="ui button">
            <i class="chevron circle up icon"></i>Şirket Ekle
          </button>
        </div>
      </div>
    </div>
    <!-- Is Outsource -->
    <div class="field sixty wide">
      <label for="IsOutsource">Şirket Taşeron mu?</label>
      <select v-model="resistance.isOutsource" @change="toggleOutsource">
        <option :value="false">Hayır</option>
        <option :value="true">Evet</option>
      </select>
    </div>

    <!-- Main Company (Conditional) -->
    <div v-if="resistance.isOutsource" id="outsource" class="sixty wide field">
      <label for="MainCompanyId">Ana Şirket</label>
      <div class="two fields">
        <div class="field">
          <select v-model="resistance.mainCompanyId">
            <option value="">--Seçiniz--</option>
            <option
                v-for="company in companies"
                :key="company.id"
                :value="company.id"
            >
              {{ company.name }}
            </option>
          </select>
        </div>
        <div class="field">
          <button type="button" @click="openCompanyModal(true)" class="ui button">
            <i class="chevron circle up icon"></i>Ana Şirket Ekle
          </button>
        </div>
      </div>
    </div>

    <!-- Employee Count -->
    <div class="two fields">
      <div class="field">
        <label for="EmployeeCountId">İş Yerindeki İşçi Sayısı</label>
        <select v-model="resistance.employeeCountId">
          <option value="">--Seçiniz--</option>
          <option
              v-for="count in employeeCounts"
              :key="count.id"
              :value="count.id"
          >
            {{ count.name }}
          </option>
        </select>
        <span v-if="formErrors.employeeCountId" class="text-danger">
          {{ formErrors.employeeCountId }}
        </span>
      </div>
      <div class="field">
        <label for="EmployeeCount">İş Yerindeki İşçi Sayısı (Tam)</label>
        <input
            type="text"
            id="EmployeeCount"
            v-model="resistance.employeeCount"
        />
      </div>
    </div>
    <div class="field">
      <label for="CorporationIds">Kurumsallık</label>
      <multiselect id="CorporationIds" v-model="resistance.corporationIds" :options="corporations"
                   :multiple="true" :close-on-select="false" :clear-on-select="false"
                   :preserve-search="true" placeholder="Seçiniz" label="name" track-by="id" :preselect-first="true"
                   :taggable="true" @tag="addTag">

      </multiselect>
    </div>
    <!-- Other Fields -->
    <div class="field">
      <label for="ResistanceResult">Sonuç</label>
      <select v-model="resistance.resistanceResult" class="ui fluid dropdown">
        <option :value="0">Bilinmiyor</option>
        <option :value="1">Tam Kazanım</option>
        <option :value="2">Yarım Kazanım</option>
        <option :value="3">Sıfır Kazanım</option>
      </select>
    </div>

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
    />

    <!-- Save and Cancel Buttons -->
    <button id="btnSave" class="ui primary button" @click.prevent="saveForm">
      KAYDET
    </button>
    <button type="button" id="btnCancelResistanceModal" class="ui negative button">
      SİL
    </button>
  </form>
  <Modal ref="modalRef"
         :companyTypes="companyTypes"
         :companyScales="companyScales"
         :worklines="worklines"
         @save-company="handleSaveCompany"/>

</template>

<script>
import {fetchWithToken} from "../../fetchWrapper";
import Multiselect from 'vue-multiselect'
import ProtestoAccordion from "../protesto/protesto-accordion.vue";
import Modal from "../Modal.vue"; // Adjust the path based on your folder structure


export default {
  name: "EditResistance",
  components: {Modal, ProtestoAccordion, Multiselect},
  data() {
    return {
      isLoading: true,
      resistance: {
        id: "",
        resistanceDescription: "",
        categoryId: "",
        selectedResistanceReasons: [],
        developRight: null,
        isOutsource: false,
        mainCompanyId: "",
        employeeCountId: "",
        employeeCount: "",
        resistanceResult: 0,
        protestos: [],
        protestoItems: []
      },
      categories: [],
      resistanceReasons: [],
      companies: [],
      corporations: [],
      employeeCounts: [],
      protestoPlaceOptions: [], 
      protestoTypeOptions: [],
      genderOptions: [],
      companyTypes: [
        { value: 1, text: "Type 1" },
        { value: 2, text: "Type 2" },
      ],
      companyScales: [
        { value: 1, text: "Small" },
        { value: 2, text: "Medium" },
      ],
      worklines: [
        { value: 1, text: "Workline 1" },
        { value: 2, text: "Workline 2" },
      ],
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
    
    fetchWithToken("/lookup/employeeCounts")
        .then(response => response.json())
        .then(data => (this.employeeCounts = data));

    fetchWithToken("/ProtestoPlace/List")
        .then(response => response.json())
        .then(data => (this.protestoPlaceOptions = data));
    fetchWithToken("/ProtestoType/List")
        .then(response => response.json())
        .then(data => (this.protestoTypeOptions = data));
    
    fetchWithToken("/lookup/genders")
        .then(response => response.json())
        .then(data => (this.genderOptions = data));
    
    fetchWithToken("/corporation/list")
        .then(response => response.json())
        .then(data => (this.corporations = data));


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
          "Content-Type": "application/json", // Ensure JSON is sent
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
    addTag (newTag) {
      const tag = {
        name: newTag,
        code: newTag.substring(0, 2) + Math.floor((Math.random() * 10000000))
      }
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

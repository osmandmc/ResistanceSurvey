<template>
  <form class="ui form">
    <Resistance
        :resistance="resistance"
        :corporations="corporations"
        :resistanceReasons="resistanceReasons"
        :employeeCounts="employeeCounts"
        :companies="companies"
        :categories="categories"
        :formErrors="formErrors"
    />
    <h3 class="ui dividing header">Eylem</h3>
    <Protesto
        :protesto="resistance.protesto"
        :genderOptions="genderOptions"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        @addLocation="handleAddLocation"
    />
    <!-- Save and Cancel Buttons -->
    <button class="ui primary button" @click.prevent="saveForm">
      KAYDET
    </button>
    <button type="button" id="btnCancelResistanceModal" class="ui negative button">
      VAZGEÇ
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
import Multiselect from 'vue-multiselect'
import CompanyModal from "../CompanyModal.vue";
import Protesto from "../protesto/Protesto.vue";
import Resistance from "./Resistance.vue";
import EditProtesto from "../protesto/EditProtesto.vue"; // Adjust the path based on your folder structure


export default {
  name: "CreateResistance",
  components: {EditProtesto, Resistance, Protesto, CompanyModal, Multiselect},
  data() {
    return {
      resistance: {
        resistanceDescription: null,
        categoryId: null,
        developRight: null,
        isOutsource: false,
        mainCompanyId: null,
        employeeCountId: null,
        employeeCount: null,
        resistanceResult: null,
        employmentTypeIds: null,
        corporationIds: null,
        resistanceReasonIds: null,
        protesto: {
          locations: []
        },
      },
      companies:[],
      resistanceReasons: [],
      categories: [],
      corporations: [],
      employeeCounts: [],
      companyTypes: [],
      companyScales: [],
      worklines: [],
      protestoPlaceOptions: [],
      protestoTypeOptions: [],
      genderOptions: [],
      employeeCountInProtestoOptions:[],
      formErrors: {},
    };
  },
  props: ['id'],  // The id is passed as a prop from the router
  mounted() {
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

    fetchWithToken("/lookup/employeeCountInProtesto")
        .then(response => response.json())
        .then(data => (this.employeeCountInProtestoOptions = data));
    
  },
  methods: {
    toggleOutsource() {
      // Show/hide outsource section
    },
    checkTradeUnion() {
      const corporationIds = this.resistance?.corporationIds?.map(s => s.id);
      const queryParams = new URLSearchParams();
      corporationIds.forEach(id => queryParams.append('corporationIds', id));

      fetchWithToken(`/corporation/checkTradeUnion?${queryParams.toString()}`)
          .then(response => response.json())
          .then(data => this.tradeUnionIncluded = data);
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
        body: JSON.stringify(companyData)
      })
          .then(response => response.json())
          .then(data => (console.log(data)));
      this.companies.push(companyData);
    },
    saveForm() {
      const resistanceData = {
        categoryId: this.resistance.categoryId,
        companyId: this.resistance.companyId,
        resistanceReasonIds: this.resistance.resistanceReasonIds,
        mainCompanyId: this.resistance.mainCompanyId,
        corporationIds: this.resistance.corporationIds,
        hasTradeUnion: true,
        tradeUnionAuthorityId: this.resistance.tradeUnionAuthorityId,
        note: this.resistance.note,
        resistanceDescription: this.resistance.resistanceDescription,
        employeeCountId: this.resistance.employeeCountId,
        employeeCount: this.resistance.employeeCount,
        tradeUnionId: this.resistance.tradeUnionId,
        protesto: this.resistance.protesto,
      }

      fetchWithToken("/Resistance/CreateResistance", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(resistanceData)
      })
      .then(response => console.log(response))
          .catch(error => console.log(error));
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date
    },
    handleAddLocation(){
      this.resistance.protesto.locations.push({});
    }
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>

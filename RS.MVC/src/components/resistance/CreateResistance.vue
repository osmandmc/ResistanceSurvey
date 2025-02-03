<template>
  <form class="ui form">
    <Resistance
        :resistance="resistance"
        :formErrors="formErrors"
       >
    </Resistance>
   
    <Protesto
        :protesto="resistance.protesto"
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
import Resistance from "./Resistance.vue"; // Adjust the path based on your folder structure


export default {
  name: "CreateResistance",
  components: {Resistance, Protesto, CompanyModal, Multiselect},
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
        protesto: {},
      },
      companies:[],
      companyTypes: [],
      companyScales: [],
      worklines: [],
      formErrors: {},
    };
  },
  props: ['id'],  // The id is passed as a prop from the router
  mounted() {
    fetchWithToken("/company/list")
        .then(response => response.json())
        .then(data => (this.companies = data));
    fetchWithToken("/lookup/companyTypes")
        .then(response => response.json())
        .then(data => (this.companyTypes = data));

    fetchWithToken("/lookup/companyScales")
        .then(response => response.json())
        .then(data => (this.companyScales = data));

    fetchWithToken("/lookup/companyWorklines")
        .then(response => response.json())
        .then(data => (this.worklines = data));
    
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
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>

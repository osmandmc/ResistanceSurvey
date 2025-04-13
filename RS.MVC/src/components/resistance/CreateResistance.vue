<template>
  <form class="ui form">
    <Resistance
        :resistance="resistance"
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
    <h3 class="ui dividing header">Eylem</h3>
    <Protesto
        :protesto="resistance.protesto"
        :genderOptions="genderOptions"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        :cities="cities"
        :districts="districts"
        :intervention-types="interventionTypes"
        :formErrors="formErrors"
        @addLocation="handleAddLocation"
        @deleteLocation="handleDeleteLocation"
        @onInputChanged="clearFormError"
    />

    <resistance-news :news="this.resistance.resistanceNews" @removeNews="handleRemoveNews"/>

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
import EditProtesto from "../protesto/EditProtesto.vue";
import ResistanceNews from "../news/ResistanceNews.vue";
import {inject} from "vue"; // Adjust the path based on your folder structure

export default {
  name: "CreateResistance",
  components: {ResistanceNews, EditProtesto, Resistance, Protesto, CompanyModal, Multiselect},
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
        resistanceNews: [],
        protesto: {
          interventionTypeIds: [],
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
      cities:[],
      districts:[],
      tradeUnionAuthorities:[],
      tradeUnions: [],
      employmentTypes: [],
      interventionTypes: [],
      formErrors: {},
      isMain: false,
    };
  },
  setup() {
    const addedNews = inject('addedNews'); // Inject full object
    return { addedNews };
  },
  props: ['id'],  // The id is passed as a prop from the router
  watch: {
    'addedNews.news': {
      handler(newNews) {
        console.log(newNews);
        // if (newNews && newNews.id && this.resistance) {
          console.log(newNews);
          this.resistance.resistanceNews.push(newNews); // Push into array
        // }
      },
      deep: true,
      // immediate: true,
    }
  },
  mounted() {
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

    fetchWithToken("/lookup/employmentTypes")
        .then(response => response.json())
        .then(data => (this.employmentTypes = data));

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

    fetchWithToken("/lookup/cities")
        .then(response => response.json())
        .then(data => (this.cities = data));

    fetchWithToken("/lookup/districts")
        .then(response => response.json())
        .then(data => (this.districts = data));

    fetchWithToken("/lookup/interventionTypes")
        .then(response => response.json())
        .then(data => (this.interventionTypes = data));
  },
  methods: {
    checkTradeUnion() {
      const corporationIds = this.resistance?.corporationIds?.map(s => s.id);
      const queryParams = new URLSearchParams();
      corporationIds.forEach(id => queryParams.append('corporationIds', id));

      fetchWithToken(`/corporation/checkTradeUnion?${queryParams.toString()}`)
          .then(response => response.json())
          .then(data => this.tradeUnionIncluded = data);
    },
    handleSaveCompany(companyData) {
      fetchWithToken("/Company/Create/", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(companyData)
      })
      .then(response => response.json())
      .then(data => {
        console.log(data);
        this.companies.push(data);
        if(this.isMain){
          this.resistance.mainCompanyId = data;  
        } else {
          this.resistance.companyId = data;  
        }
        
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
        employmentTypeIds: this.resistance.employmentTypeIds,
        protesto: this.resistance.protesto,
        resistanceNews: this.resistance.resistanceNews,
        firedEmployeeCountByProtesto: this.resistance.protesto.firedEmployeeCountByProtesto,
        resistanceResult: this.resistance.resistanceResult,
      }

      fetchWithToken("/ResistanceApi/CreateResistance", {
        method: 'POST',
        headers: {
          "Content-Type": "application/json", // Ensure JSON is sent
        },
        body: JSON.stringify(resistanceData)
      })
      .then(response =>
          {
            this.isLoading = false;
            if(response.status === 400) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: response.statusText,
              });
            }
            else if(response.status === 500) {
              this.$swal.fire({
                icon: "error",
                title: "Bir hata olustu",
                text: 'Yazilim destek: ' + response.statusText,
              });
            }
            else {
              this.$swal('Vaka kaydedildi');
              this.$router.push({ path: `/` });
            }
          }
      )
      .catch(error =>
      {
        this.isLoading = false;
        this.$swal.fire({
          icon: "error",
          title: "Bir hata olustu",
        });
        console.log(error)
      });
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date
    },
    handleRemoveNews(newsId){
      console.log('remove newsId ',newsId);
      this.resistance.resistanceNews = this.resistance.resistanceNews.filter(s=>s.id !== newsId);
    },
    handleOpenCompanyModal(isMain) {
      console.log(isMain);
      this.isMain = isMain;
      this.$refs.modalRef.openModal();
    },
    handleAddLocation(){
      this.resistance.protesto.locations.push({id:0, protestoId: 0, cityId:null, districtId: null, place: null, deleted: false});
    },
    handleDeleteLocation(index) {
      const location = this.resistance.protesto.locations[index];
      location.deleted = true;
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

      // GenderId: Required
      // if (!this.resistance.genderId) {
      //   errors.genderId = "Lütfen bir cinsiyet giriniz.";
      // }

      // ProtestoTypeIds: At least one protest type selected
      if (!this.resistance.protesto.protestoTypeIds || this.resistance.protesto.protestoTypeIds.length === 0) {
        errors.protestoTypeIds = "Lütfen en az bir eylem türü seçiniz.";
      }

      // ProtestoPlaceIds: At least one protest place selected
      if (!this.resistance.protesto.protestoPlaceIds || this.resistance.protesto.protestoPlaceIds.length === 0) {
        errors.protestoPlaceIds = "Lütfen en az bir eylem mekanı seçiniz.";
      }

      // ProtestoStartDate: Required
      if (!this.resistance.protesto.protestoStartDate) {
        errors.protestoStartDate = "Lütfen başlangıç tarihi seçiniz.";
      }

      // AnyLegalIntervention: Required
      // if (this.resistance.protesto.anyLegalIntervention === null || this.resistance.protesto.anyLegalIntervention === undefined) {
      //   errors.anyLegalIntervention = "Hukuki girişim var mı?";
      // }

      // DevelopRight: Required
      if (this.resistance.developRight === null || this.resistance.developRight === undefined) {
        errors.developRight = "Hak Geliştirmeye/Savunma Özelliği";
      }
      // ProtestoStartDate: Required
      if (!this.resistance.protesto.protestoStartDate) {
        errors.protestoStartDate = "Lütfen başlangıç tarihi seçiniz.";
      }

      // GenderId: Required
      if (!this.resistance.protesto.genderId) {
        errors.genderId = "Lütfen bir cinsiyet giriniz.";
      }
      
      // InterventionTypeIds: At least one intervention type selected
      if (!this.resistance.protesto.interventionTypeIds || this.resistance.protesto.interventionTypeIds.length === 0) {
        errors.interventionTypeIds = "Lütfen en az bir müdahale tipi seçiniz.";
      }

      // CustodyCount: Required if custody is possible
      if (this.resistance.protesto.isCustodyPossible && !this.resistance.protesto.custodyCount) {
        errors.custodyCount = "Lütfen gözaltı sayısını giriniz.";
      }
      if (this.resistance.resistanceResult === null || this.resistance.resistanceResult === undefined) {
        errors.resistanceResult = "Lütfen sonuç giriniz.";
      }
      return errors;
    },
    clearFormError(field){
      this.formErrors[field] = "";
    }
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>

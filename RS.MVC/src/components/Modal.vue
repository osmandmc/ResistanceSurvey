<template>
  <div ref="modal" class="ui modal">
    <div class="header">
      Şirket Formu
    </div>
    <div class="content">
      <form class="ui form" id="companyForm">
        <!-- Hidden Input -->
        <input type="hidden" v-model="formData.IsMain" />

        <!-- Company Name Field -->
        <div class="field">
          <label for="CompanyName">Şirket Adı</label>
          <input
              type="text"
              id="CompanyName"
              v-model="formData.CompanyName"
              placeholder="Şirket adı giriniz"
          />
        </div>

        <!-- Dropdown Fields -->
        <div class="three fields">
          <!-- TypeId Dropdown -->
          <div class="field">
            <label for="TypeId">Şirketin Niteliği</label>
            <select
                id="TypeId"
                v-model="formData.TypeId"
                :disabled="!companyTypes.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="type in companyTypes" :key="type.value" :value="type.value">
                {{ type.text }}
              </option>
            </select>
          </div>

          <!-- ScaleId Dropdown -->
          <div class="field">
            <label for="ScaleId">Şirketin Büyüklüğü</label>
            <select
                id="ScaleId"
                v-model="formData.ScaleId"
                :disabled="!companyScales.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="scale in companyScales" :key="scale.value" :value="scale.value">
                {{ scale.text }}
              </option>
            </select>
          </div>

          <!-- WorklineId Dropdown -->
          <div class="field">
            <label for="WorklineId">Şirketin İş Kolu</label>
            <select
                id="WorklineId"
                v-model="formData.WorklineId"
                :disabled="!worklines.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="workline in worklines" :key="workline.value" :value="workline.value">
                {{ workline.text }}
              </option>
            </select>
          </div>
        </div>

        <!-- Conditional Button -->
        <button
            v-if="formData.IsMain"
            id="btnAddMainCompany"
            class="ui button primary"
            @click.prevent="addMainCompany"
        >
          Ana Şirket Ekle
        </button>
        <button
            v-else
            id="btnAddCompany"
            class="ui button positive"
            @click.prevent="addCompany"
        >
          Şirket Ekle
        </button>
      </form>
    </div>
  </div>
</template>

<script>
export default {
  name: "Modal",
  props: {
    // Data passed from the parent, e.g., API dropdown options
    companyTypes: {
      type: Array,
      default: () => [],
    },
    companyScales: {
      type: Array,
      default: () => [],
    },
    worklines: {
      type: Array,
      default: () => [],
    }
  },
  data() {
    return {
      formData: {
        IsMain: false,
        CompanyName: "",
        TypeId: "",
        ScaleId: "",
        WorklineId: "",
      },
    };
  },
  methods: {
    openModal() {
      console.log(this.formData);
      $(this.$refs.modal).modal("show");
    },
    closeModal() {
      $(this.$refs.modal).modal("hide");
    },
    addMainCompany() {
      console.log("Adding Main Company:", this.formData);
      
      this.closeModal();
    },
    addCompany() {
      console.log("Adding Company:", this.formData);
      const company = {
        isMain: true,
        name: this.formData.CompanyName,
        typeId: this.formData.TypeId,
        scaleId: this.formData.ScaleId,
        worklineId: this.formData.WorklineId,
      }
      const company1 = {
        name: this.formData.CompanyName,
        typeId: 1,
        scaleId: 2,
        worklineId: 3,
        isMain: false
      }
      this.$emit("save-company", company1);
      this.closeModal();
    }
  },
};
</script>

<style scoped>
/* Add any scoped styles for the modal if needed */
</style>

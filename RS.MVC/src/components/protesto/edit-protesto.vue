<template>
  <div class="ui form">
    <!-- Protesto Types -->
    <div class="field">
      <label for="ProtestoTypeIds">Eylem Türleri</label>
     
      <multiselect
          id="ProtestoTypeIds"
          v-model="protesto.protestoTypeIds"
          :options="protestoTypeOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
      ></multiselect>
    </div>

    <!-- Start and End Dates -->
    <div class="two fields">
      <div class="field">
        <label for="ProtestoStartDate">Başlangıç Tarihi</label>
        <VueDatePicker v-model="protesto.protestoStartDate"></VueDatePicker>
      </div>
      <div class="field">
        <label for="ProtestoEndDate">Bitiş Tarihi</label>
        <VueDatePicker v-model="protesto.protestoEndDate"></VueDatePicker>
      </div>
    </div>

    <!-- Protesto Places -->
    <div class="field">
      <label for="ProtestoPlaceIds">Yerler</label>
      <multiselect
          id="ProtestoPlaceIds"
          v-model="protesto.protestoPlaceIds"
          :options="protestoPlaceOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
      ></multiselect>
    </div>

    <!-- Gender -->
    <div class="field">
      <label for="GenderId">Cinsiyet</label>
      <select v-model="protesto.genderId" id="GenderId">
        <option value="">--Seçiniz--</option>
        <option
            v-for="gender in genderOptions"
            :key="gender.id"
            :value="gender.id"
        >
          {{ gender.name }}
        </option>
      </select>
    </div>

    <!-- Other Fields -->
    <div class="two fields">
      <div class="field">
        <label for="CustodyCount">Gözaltı Sayısı</label>
        <input
            type="number"
            id="CustodyCount"
            v-model="protesto.custodyCount"
        />
      </div>
      <div class="field">
        <label for="EmployeeCountInProtesto">Eylemdeki İşçi Sayısı</label>
        <input
            type="number"
            id="EmployeeCountInProtesto"
            v-model="protesto.employeeCountInProtesto"
        />
      </div>
    </div>

    <!-- Notes -->
    <div class="field">
      <label for="Note">Notlar</label>
      <textarea id="Note" v-model="protesto.note" rows="3"></textarea>
    </div>

    <!-- Save Button -->
    <button
        class="ui primary button"
        type="button"
        @click="saveProtesto"
    >
      Kaydet
    </button>
  </div>
</template>

<script>
import Multiselect from "vue-multiselect";
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css'

export default {
  name: "EditProtesto",
  components: { Multiselect, VueDatePicker },
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
  },
  mounted() {
    
  },
  methods: {
    saveProtesto() {
      this.$emit("saveProtesto", this.protesto);
    },
  },
};
</script>

<style scoped>
/* Add any scoped styles here if necessary */
</style>

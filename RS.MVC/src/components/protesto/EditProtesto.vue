<template>
  <div class="ui form">
    <Protesto 
        :protesto="protesto"
        :protestoTypeOptions="protestoTypeOptions"
        :protestoPlaceOptions="protestoPlaceOptions"
        :genderOptions="genderOptions"
        :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
    />
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
import Protesto from "./Protesto.vue";
import '@vuepic/vue-datepicker/dist/main.css'

export default {
  name: "EditProtesto",
  components: { Protesto },
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

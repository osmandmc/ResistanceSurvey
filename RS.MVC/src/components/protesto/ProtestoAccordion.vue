<template>
  <div class="ui styled accordion">
    <div v-for="(protesto, index) in protestoItems" :key="protesto.protestoId">
      <div class="title" @click="toggleAccordion(index)">
        <i class="dropdown icon"></i>
        <span v-if="protesto.protestoId !== 0">
          {{ protesto?.protestoTypeIds[0]?.name }} | 
          {{ formatDate(protesto?.protestoStartDate) }} 
        </span>
      </div>
      <div :class="['content', { active: activeIndex === index }]">
        <edit-protesto
            :protesto="protesto"
            :genderOptions="genderOptions"
            :protestoTypeOptions="protestoTypeOptions"
            :protestoPlaceOptions="protestoPlaceOptions"
            :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
            :cities="cities"
            :districts="districts"
            :intervention-types="interventionTypes"
            @cancelProtesto="handleCancelProtesto"
            @onProtestoAdded="handleSaveProtesto"
            @onProtestoDeleted="handleDeleteProtesto"
        />
      </div>
    </div>
  </div>
</template>

<script>

import EditProtesto from "./EditProtesto.vue";
import Resistance from "../resistance/Resistance.vue";

export default {
  name: "ProtestoAccordion",
  emits: ["cancelProtesto", "saveProtesto", "deleteProtesto", "onChangeActiveProtesto", "onInputChanged"],
  components: {Resistance, EditProtesto },
  data() {
    return {
      activeIndex: this.activeProtestoIndex,
    }
  },
  props: {
    activeProtestoIndex: {
      type: Number,
      default: () => null
    },
    protestoItems: {
      type: Array,
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
    },
    interventionTypes: {
      type: Array,
      required: true,
    }
  },
  watch: {
    activeProtestoIndex(newVal) {
      console.log(newVal);
      this.activeIndex = newVal;
    }
  },
  methods: {
    toggleAccordion(index) {
      console.log(index);
      this.activeIndex = this.activeIndex === null ? index : null;
      console.log(this.activeIndex);
      this.$emit("onChangeActiveProtesto", this.activeIndex);
    },
    handleCancelProtesto() {
      this.activeIndex = null;
      this.$emit('cancelProtesto');
    },
    handleSaveProtesto(protesto) {
      this.$emit('saveProtesto', protesto);
    },
    handleDeleteProtesto(id) {
      console.log('delete', id);
      this.$emit('deleteProtesto', id);
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date to a readable string
    },

  },
};
</script>

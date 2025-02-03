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
            @cancelProtesto="handleCancelProtesto"
        />
      </div>
    </div>
  </div>
</template>

<script>

import EditProtesto from "./EditProtesto.vue";

export default {
  name: "ProtestoAccordion",
  emits: ["cancelProtesto"],
  components: { EditProtesto },
  data() {
    return {
      activeIndex: this.activeProtestoIndex
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
      default: () => []
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
    }
  },
  watch: {
    activeProtestoIndex(newVal) {
      console.log(newVal);
      this.activeIndex = newVal;
    }
  },
  mounted() {
    console.log(this.activeProtestoIndex);
  },
  methods: {
    toggleAccordion(index) {
      console.log("activeIndex", this.activeIndex);
      console.log("activeProtestoIndex", this.activeProtestoIndex);
      this.activeIndex = this.activeIndex === index ? index : null;
    },
    saveProtesto(updatedProtesto) {
      this.$emit("updateProtesto", updatedProtesto);
    },
    handleCancelProtesto(protesto) {
      console.log("cancel", protesto);
      this.$emit('cancelProtesto', protesto);
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date to a readable string
    },
  },
};
</script>

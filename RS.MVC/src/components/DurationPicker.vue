<template>
  <div class="duration-picker">
    <!-- Days -->
    <div v-if="showDays" class="duration-block">
      <input
          type="number"
          min="0"
          v-model.number="days"
          :disabled="disabled"
          @change="emitChange"
      />
      <span>Gün</span>
    </div>

    <!-- Hours -->
    <div class="duration-block">
      <input
          type="number"
          :min="0"
          :max="showDays ? 23 : 99999"
          v-model.number="hours"
          :disabled="disabled"
          @change="emitChange"
      />
      <span>Saat</span>
    </div>

    <!-- Minutes -->
    <div class="duration-block">
      <input
          type="number"
          min="0"
          max="59"
          v-model.number="minutes"
          :disabled="disabled"
          @change="emitChange"
      />
      <span>Dakika</span>
    </div>

    <!-- Seconds -->
    <div v-if="showSeconds" class="duration-block">
      <input
          type="number"
          min="0"
          max="59"
          v-model.number="seconds"
          :disabled="disabled"
          @change="emitChange"
      />
      <span>Saniye</span>
    </div>
  </div>
</template>

<script>
export default {
  name: "DurationPicker",
  props: {
    /**
     * Total duration in seconds (used by v-model).
     */
    modelValue: {
      type: Number,
      default: 0,
    },
    /**
     * Whether to show the days input.
     */
    showDays: {
      type: Boolean,
      default: true,
    },
    /**
     * Whether to show the seconds input.
     */
    showSeconds: {
      type: Boolean,
      default: false,
    },
    /**
     * If true, the inputs are disabled.
     */
    disabled: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      days: 0,
      hours: 0,
      minutes: 0,
      seconds: 0,
    };
  },
  watch: {
    // Sync internal fields whenever modelValue changes from the outside
    modelValue: {
      immediate: true,
      handler(newVal) {
        this.setValueFromSeconds(newVal);
      },
    },
  },
  methods: {
    /**
     * Parse total seconds into days, hours, minutes, seconds.
     */
    setValueFromSeconds(totalSeconds) {
      let total = parseInt(totalSeconds, 10);
      if (isNaN(total)) total = 0;

      this.seconds = total % 60;
      total = Math.floor(total / 60);
      this.minutes = total % 60;
      total = Math.floor(total / 60);

      if (this.showDays) {
        this.hours = total % 24;
        this.days = Math.floor(total / 24);
      } else {
        // If not showing days, just accumulate hours beyond 24
        this.hours = total;
        this.days = 0;
      }
    },

    /**
     * Emit the updated total in seconds when the user edits.
     */
    emitChange() {
      const total =
          this.days * 24 * 3600 +
          this.hours * 3600 +
          this.minutes * 60 +
          this.seconds;

      this.$emit("update:modelValue", total);
    },
  },
};
</script>

<style scoped>
.duration-picker {
  display: flex;
  gap: 1rem;
}
.duration-block {
  display: inline-flex;
  flex-direction: column;
  align-items: flex-start;
}
.duration-block input {
  width: 4em;
}
</style>

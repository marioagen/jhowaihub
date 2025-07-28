<template>
  <div class="input-group">
    <Field
      :type="show ? 'text' : 'password'"
      class="form-control form-control-sm"
      :id="name"
      :ref="name"
      autocomplete="off"
      :name="name"
      :rules="rules"
      :model-value="modelValue"
      @update:model-value="$emit('update:modelValue', $event)"
      :placeholder="placeholder"
    />
    <span class="input-group-text" style="cursor:pointer" @click="show = !show">
      <LucideIcon :icon="show ? 'EyeOff' : 'Eye'" :size="12"/>
    </span>
  </div>
  <ErrorMessage :name="name" class="invalid-feedback d-block" />
</template>

<script>
import { Field, ErrorMessage } from "vee-validate";

export default {
  name: "PasswordInputComponent",
  components: {
    Field,
    ErrorMessage,
  },
  props: {
    modelValue: {
      type: String,
      default: "",
    },
    placeholder: {
      type: String,
      default: "",
    },
    rules: {
      type: [String, Object],
      default: "",
    },
    name: {
      type: String,
      required: true,
    },
  },
  emits: ["update:modelValue"],
  data() {
    return {
      show: false,
    };
  },
};
</script>
<style scoped>
.input-group input {
    border-right-width: 0px;
}
.input-group span {
    border-left-width: 0px;
}
</style>

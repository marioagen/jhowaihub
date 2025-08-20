<template>
    <div class="card shadow-sm rounded-3 overflow-hidden">
        <div
            class="d-flex justify-content-between align-items-center px-3 py-2"
            :style="{ backgroundColor: isLast ? '#E8FFE8' : '#e8f1ff' }"
        >
            <div class="d-flex align-items-center">
                <div class="d-flex flex-column align-items-start">
                    <div class="d-flex align-items-center mb-1">
                        <div
                            class="d-flex justify-content-center align-items-center rounded-circle text-white me-2"
                            style="width: 28px; height: 28px; background-color: #2F80ED;"
                        >
                            {{ index }}
                        </div>
                        <Field 
                            :name="`steps[${index - 1}].name`" 
                            rules="required" 
                            v-slot="{ field, errors }"
                            ref="titleField"
                        >
                            <div class="d-flex flex-column">
                                <input
                                    type="text"
                                    class="input-title"
                                    v-bind="field"
                                    @blur="stopEditingTitle"
                                    @keyup.enter="stopEditingTitle"
                                    @input="onTitleInput"
                                    autofocus
                                    placeholder="Title"
                                />
                                <span v-if="errors[0]" class="validation-message text-danger mt-1">
                                    {{ errors[0] }}
                                </span>
                            </div>
                        </Field>
                    </div>
                </div>
            </div>
            <button 
                type="button" 
                class="btn btn-link btn-sm"
                @click="remove"
            >
                <LucideIcon icon="X"/>
            </button>
        </div>
        <div class="card-body">
           <div class="mb-3">
                <label class="form-label text-muted small">{{ $t("workflow.status") }}</label>
                <Field 
                    :name="`steps[${index - 1}].status`" 
                    rules="required" 
                    v-slot="{ field, errors }"
                    ref="statusField"             
                >
                    <div class="d-flex flex-column">
                        <select 
                            class="form-select form-select-sm" 
                            v-bind="field"
                            @change="$emit('update-step', { ...step, status: $event.target.value })"
                        >
                            <option value="">Select status</option>
                            <option v-for="s in statusList" :key="s.id" :value="s.id">{{ s.name }}</option>
                        </select>
                        <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                    </div>
                </Field>
            </div>

            <div class="mb-2">
                <label class="form-label text-muted small">{{ $t("workflow.profiles") }}</label>
                <Field 
                    :name="`steps[${index - 1}].profile`" 
                    rules="required" 
                    v-slot="{ field, errors }"
                    ref="profileField"                    
                >
                    <div class="d-flex flex-column">
                        <div class="input-group">
                            <span class="input-group-text border-end-0 bg-white">
                                <LucideIcon icon="Users" size="16" />
                            </span>

                            <select
                                class="form-select form-select-sm border-start-0 flex-grow-1"
                                v-bind="field"
                                @change="$emit('update-step', { ...step, profile: $event.target.value })"
                            >
                                <option value="">{{ $t("workflow.responsableTeam") }}</option>
                                <option v-for="p in profilesList" :key="p.id" :value="p.id">
                                    {{ p.text }}
                                </option>
                            </select>
                        </div>
                        <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                    </div>
                </Field>
            </div>
        </div>
    </div>
</template>

<script>
    import { Field } from "vee-validate";
    export default {
        name: "WorkflowStepComponent",
        components: { 
            Field,
        },
        props: {
            step: {
                type: Object,
                required: true
            },
            index: {
                type: Number,
                required: true
            },
            isLast: {
                type: Boolean,
                default: false
            },
            profilesList: {
                type: Array,
                required: true,
            },
            statusList: {
                type: Array,
                required: true,
            },
        },
        data() {
            return {
                editingTitle: false,
                titleDebounceTimer: null,
            };
        },
        methods: {
            remove() {
                this.$emit('remove-step');
            },
            startEditingTitle() {
                this.editingTitle = true;
            },
            stopEditingTitle() {
                this.editingTitle = false;
            },
            async validateStep() {
                const titleValid = await this.$refs.titleField.validate?.();
                const statusValid = await this.$refs.statusField.validate?.();
                const profileValid = await this.$refs.profileField.validate?.();
                return titleValid?.valid && statusValid?.valid && profileValid?.valid;
            },
            onTitleInput(e) {
                const val = e.target.value;
                clearTimeout(this.titleDebounceTimer);
                this.titleDebounceTimer = setTimeout(() => {
                    this.$emit('update-step', { ...this.step, name: val });
                }, 300);
            },
        },
        beforeUnmount() {
            clearTimeout(this.titleDebounceTimer);
        }
    };
</script>

<style scoped>
.card {
  max-width: 100%;
  border: 1px solid #dee2e6;
}
.btn-close {
  background: none;
  border: none;
}
.input-title {
  border: none;
  background: transparent;
  padding: 0;
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  width: auto;
  min-width: 30px;
}

.input-title:focus {
  outline: none;
  box-shadow: none;
}
</style>

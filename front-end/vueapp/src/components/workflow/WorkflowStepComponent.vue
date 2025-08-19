<template>
    <div class="card shadow-sm rounded-3 overflow-hidden">
        <div
            class="d-flex justify-content-between align-items-center px-3 py-2"
            :style="{ backgroundColor: isLast ? '#E8FFE8' : '#e8f1ff' }"
        >
            <div class="d-flex align-items-center">
                <div
                    class="d-flex justify-content-center align-items-center rounded-circle text-white me-2"
                    style="width: 28px; height: 28px; background-color: #2F80ED;"
                >
                    {{ index }}
                </div>
                <span
                    v-if="!editingTitle"
                    class="fw-semibold text-dark small"
                    @click="startEditingTitle"
                    style="cursor: pointer;"
                >
                    {{ localStep.name || 'Etapa sem título' }}
                </span>
                <Field v-else :name="`steps[${index - 1}].name`" rules="required" v-slot="slotProps">
                    <input
                        type="text"
                        class="form-control form-control-sm"
                        v-bind="slotProps.field"
                        @blur="stopEditingTitle"
                        @keyup.enter="stopEditingTitle"
                        style="max-width: 200px;"
                        autofocus
                    />
                    <span class="validation-message text-danger" v-if="slotProps.errors?.length">
                        {{ slotProps.errors[0] }}
                    </span>
                </Field>
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
                <Field :name="`steps[${index - 1}].statusId`" rules="required" v-slot="slotProps">
                    <select
                        class="form-select form-select-sm border-start-0"
                        v-bind="slotProps.field"
                    >
                        <option value="">{{ $t("workflow.status") }}</option>
                        <option
                            v-for="(item, i) in statusList"
                            :key="i"
                            :value="item.id"
                        >
                            {{ item.id }} - {{ item.name }}
                        </option>
                    </select>
                    <span class="validation-message text-danger" v-if="slotProps.errors?.length">
                        {{ slotProps.errors[0] }}
                    </span>
                </Field>
            </div>

            <div class="mb-2">
                <label class="form-label text-muted small">{{ $t("workflow.profiles") }}</label>
                <div class="input-group">
                    <span class="input-group-text border-end-0 bg-white">
                        <LucideIcon icon="Users" size="16" />
                    </span>
                    <Field :name="`steps[${index - 1}].profileId`" rules="required" v-slot="slotProps">
                        <select
                            class="form-select form-select-sm border-start-0"
                            v-bind="slotProps.field"
                        >
                            <option value="">{{ $t("workflow.responsableTeam") }}</option>
                            <option
                                v-for="(item, i) in profilesList"
                                :key="i"
                                :value="item.id"
                            >
                                {{ item.id }} - {{ item.text }}
                            </option>
                        </select>
                        <span class="validation-message text-danger" v-if="slotProps.errors?.length">
                            {{ slotProps.errors[0] }}
                        </span>
                    </Field>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { Field, useForm } from "vee-validate";
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
                localStep: {
                    statusId: this.step.status || '',
                    profileId: this.step.profile || '',
                    name: this.step.title || '',
                    order: this.index
                },
            };
        },
        watch: {
            localStep: {
                deep: true,
                handler(newVal) {
                    this.$emit('update-step', newVal);
                }
            }
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
            validateStep() {
                const { name, statusId, profileId } = this.localStep;
                const valid = !!name && !!statusId && !!profileId;

                if (!valid) {
                    this.$notify({
                        title: 'Workflow',
                        message: 'Campos da etapa estão inválidos',
                        variant: 'danger',
                        icon: 'CircleX'
                    });
                }

                this.$emit('update-step', this.localStep);
                return valid;
            }
        },
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
</style>

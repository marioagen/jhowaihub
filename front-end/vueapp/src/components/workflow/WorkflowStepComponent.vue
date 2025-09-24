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
                            v-model="titleComputed"
                            v-slot="{ field, errors }"
                            ref="titleField"
                        >
                            <div class="d-flex flex-column">
                                <input
                                    type="text"
                                    class="input-title"
                                    v-bind="field"
                                    @blur="(e) => { field.onBlur(e); flushTitle(e) }"
                                    @keyup.enter="flushTitle($event)"
                                    placeholder="Title"
                                    autofocus
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
                @click="removeStep"
            >
                <LucideIcon icon="X"/>
            </button>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="mb-3">
                     <label class="form-label text-muted small">{{ $t("workflow.status") }}</label>
                     <Field
                         :name="`steps[${index - 1}].statusId`"
                         rules="required"
                         v-model="statusIdComputed"
                         v-slot="{ field, errors }"
                         ref="statusField"
                     >
                         <div class="d-flex flex-column">
                             <select class="form-select form-select-sm" v-bind="field">
                                 <option value="">Select status</option>
                                 <option v-for="s in statusList" :key="s.id" :value="String(s.id)">{{ s.name }}</option>
                             </select>
                             <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                         </div>
                     </Field>
                </div>                
            </div>
            <div class="row">
                <div class="mb-2">
                    <label class="form-label text-muted small">{{ $t("workflow.profiles") }}</label>
                    <Field
                        :name="`steps[${index - 1}].profileId`"
                        rules="required"
                        v-model="profileIdComputed"
                        v-slot="{ field, errors }"
                        ref="profileField"
                    >
                        <div class="d-flex flex-column">
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Users" :size="16" />
                                </span>
                                <select class="form-select form-select-sm border-start-0 flex-grow-1" v-bind="field">
                                    <option value="">{{ $t("workflow.profiles") }}</option>
                                    <option v-for="p in profilesList" :key="p.id" :value="String(p.id)">
                                        {{ p.text }}
                                    </option>
                                </select>
                            </div>
                            <span v-if="errors[0]" class="text-danger small mt-1">{{ errors[0] }}</span>
                        </div>
                    </Field>
                </div>
            </div>
            <div v-if="isEdit" class="row mt-3">
                <div class="col-12 d-flex align-items-center justify-content-between">
                    <p class="mb-0">Automação de Documentos</p>
                    <div class="d-flex">
                        <button 
                            type="button" 
                            class="btn-outline-primary btn-table btn-sm table-btn"
                            @click="redirectToFlow"
                        >
                            <LucideIcon icon="SquarePen" :size="15" class="me-1" />
                        </button>
                        <button 
                            type="button" 
                            class="btn-outline-danger btn-table btn-sm table-btn"
                            @click="removeFlow"
                        >
                            <LucideIcon icon="Trash" :size="15" class="me-1" />
                        </button>
                    </div>
                </div>
            </div>
            <div v-else class="row mt-3">
                <div class="col-12">
                    <button 
                        class="btn btn-outline-primary btn-sm w-100"
                        @click="redirectToFlow"
                    >
                        <LucideIcon icon="Workflow" :size="15" />
                        Automação de Documentos
                    </button>
                </div>
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
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data() {
            return {
                editingTitle: false,
                titleDebounceTimer: null,
            };
        },
        computed: {
            titleComputed: {
                get() {
                    return this.step?.name ?? "";
                },
                set(val) {
                    clearTimeout(this.titleDebounceTimer);
                    this.titleDebounceTimer = setTimeout(() => {
                        this.$emit("update-step", { ...this.step, name: val });
                    }, 300);
                },
            },
            statusIdComputed: {
                get() {
                    return String(this.step?.statusId ?? "");
                },
                set(val) {
                    this.$emit("update-step", { ...this.step, statusId: String(val) });
                },
            },
            profileIdComputed: {
                get() {
                    return String(this.step?.profileId ?? "");
                },
                set(val) {
                    this.$emit("update-step", { ...this.step, profileId: String(val) });
                },
            },
        },
        methods: {
            removeStep() {
                this.$emit("remove-step", { ...this.step, isActive: false });
            },
            startEditingTitle() {
                this.editingTitle = true;
            },
            stopEditingTitle() {
                this.editingTitle = false;
            },
            flushTitle(e) {
                clearTimeout(this.titleDebounceTimer);
                const val = e?.target?.value ?? "";
                this.$emit("update-step", { ...this.step, name: val });
                this.stopEditingTitle();
            },
            async validateStep() {
                this.$refs.titleField?.setTouched?.(true);
                this.$refs.statusField?.setTouched?.(true);
                this.$refs.profileField?.setTouched?.(true);

                const [titleValid, statusValid, profileValid] = await Promise.all([
                    this.$refs.titleField?.validate?.(),
                    this.$refs.statusField?.validate?.(),
                    this.$refs.profileField?.validate?.(),
                ]);
                return titleValid?.valid && statusValid?.valid && profileValid?.valid;
            },
            redirectToFlow() {
                this.$emit("saveWorkflow");
                if(this.isEdit) {
                    return this.$router.push({
                        name: 'EditFlow',
                        params: {
                            id: 1,
                        },
                    });
                }
                this.$router.push({ name: 'NewFlow' });
            },
            removeFlow() {
                //remove the given flow endpoint
            },
        },
        beforeUnmount() {
            clearTimeout(this.titleDebounceTimer);
        }
    }
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

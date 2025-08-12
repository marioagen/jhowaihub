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
                <input
                    v-else
                    type="text"
                    v-model="localStep.name"
                    class="form-control form-control-sm"
                    @blur="stopEditingTitle"
                    @keyup.enter="stopEditingTitle"
                    style="max-width: 200px;"
                    autofocus
                />
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
                <select
                        class="form-select form-select-sm border-start-0"
                        v-model="localStep.statusId"
                    >
                        <option value="">{{ $t("workflow.status") }}</option>
                        <option 
                            v-for="(item, index) in statusList" 
                            :key="index"
                            :value="item.id" 
                        >
                            {{ item.id }} - {{ item.name }}
                        </option>
                    </select>
            </div>

            <div class="mb-2">
                <label class="form-label text-muted small">{{ $t("workflow.profiles") }}</label>
                <div class="input-group">
                    <span class="input-group-text border-end-0 bg-white">
                        <LucideIcon icon="Users" size="16" />
                    </span>
                    <select
                        class="form-select form-select-sm border-start-0"
                        v-model="localStep.profileId"
                    >
                        <option value="">{{ $t("workflow.responsableTeam") }}</option>
                        <option 
                            v-for="(item, index) in profilesList" 
                            :key="index"
                            :value="item.id" 
                        >
                            {{ item.id }} - {{ item.text }}
                        </option>
                    </select>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "WorkflowStepComponent",
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

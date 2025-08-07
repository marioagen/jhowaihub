<template>
    <div class="card shadow-sm rounded-3 overflow-hidden">
        <!-- Header -->
        <div class="d-flex justify-content-between align-items-center px-3 py-2" style="background-color: #e8f1ff;">
            <div class="d-flex align-items-center">
                <div
                    class="d-flex justify-content-center align-items-center rounded-circle text-white me-2"
                    style="width: 28px; height: 28px; background-color: #2F80ED;"
                >
                    {{ index }}
                </div>
                <span class="fw-semibold text-dark small">
                    {{ localStep.profile || 'Etapa sem perfil' }}
                </span>
            </div>
            <button 
                type="button" 
                class="btn btn-link btn-sm"
                @click="remove"
            >
                <LucideIcon icon="X"/>
            </button>
        </div>

        <!-- Body -->
        <div class="card-body">
            <div class="mb-3">
                <label class="form-label text-muted small">STATUS</label>
                <select class="form-select" v-model="localStep.status">
                    <option value="analizado">Analisado</option>
                    <option value="pendente">Pendente</option>
                    <option value="rejeitado">Rejeitado</option>
                </select>
            </div>

            <div class="mb-2">
                <label class="form-label text-muted small">PERFIL RESPONSÁVEL</label>
                <div class="input-group">
                    <span class="input-group-text border-end-0 bg-white">
                        <LucideIcon icon="Users" size="16" />
                    </span>
                    <select class="form-select border-start-0" v-model="localStep.profile">
                        <option value="senior">Analista Sênior</option>
                        <option value="junior">Analista Júnior</option>
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
            }
        },
        data() {
            return {
                localStep: {
                    status: this.step.status || '',
                    profile: this.step.profile || ''
                }
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
            }
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
</style>

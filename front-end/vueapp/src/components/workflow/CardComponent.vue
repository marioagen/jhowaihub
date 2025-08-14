<template>
        <div class="card">
            <div class="card-body">
                <p>{{dataCard.name}}</p>
                <div class="mb-2">
                    <LucideIcon icon="FileText" size="12" class="me-1" />
                    <small>{{dataCard.description}}</small>
                </div>
                <div class="mb-2">
                    <LucideIcon icon="Calendar" size="12" class="me-1" />
                    <small>{{dataCard.created}}</small>
                </div>
                <hr>
                <div class="mb-2">
                    <LucideIcon icon="User" size="12" class="me-1" />
                    <small>{{dataCard.owner}}</small>
                </div>
                <div class="mb-2">
                    <button class="btn btn-sm btn-primary" style="float:right" @click="advanceStep" v-if="!isLastStep">
                        <span>{{ verifyFirst }}</span>
                        <LucideIcon icon="ChevronRight" size="16" class="me-1" />
                    </button>
                    <div class="badge" :style="badgeStyle(dataStep.status.color)">{{dataStep.status.name}}</div>
                </div>
            </div>
        </div>
</template>

<script>
    import DocumentsServices from "@/services/documents/DocumentsServices.js";
    import CardsServices from "@/services/cards/CardsServices";
    export default {
        name: "CardComponent",
        props: {
            dataCard: {
                type: Object,
                required: true,
                default: () => {},
            },
            dataStep: {
                type: Object,
                required: true,
                default: () => { },
            },
            isFirstStep: {
                type: Boolean,
                required: true,
                default: false,
            },
            isLastStep: {
                type: Boolean,
                required: true,
                default: false,
            }
        },
        methods: {
            badgeStyle(color) {
                return {
                    '--cor-base': color,
                    color: 'var(--cor-base)',
                    backgroundColor: 'color-mix(in srgb, var(--cor-base) 30%, white)'
                };
            },
            advanceStep() {
                if (this.isFirstStep) {
                    this.getDocumentNormalized();
                }
                else {
                    this.updateStatus();
                }
            },
            getDocumentNormalized() {
                let paramsReq = {
                    Id: parseInt(this.dataCard.documentId),
                    Embeddings_model_name: "",
                };
                DocumentsServices.normalizeDocument(paramsReq)
                        .then((response) => {
                            if (response.error !== undefined) {
                                console.log(response.error);
                            }
                            this.updateStatus()
                        })
                        .finally(() => {
                        });
            },
            updateStatus() {
                if (!this.isLastStep) {
                    var params = {
                        CardId: this.dataCard.id,
                        NextStepOrder: this.dataStep.order + 1,
                        WorkflowId: this.dataStep.workflowId,
                    }
                    CardsServices.updateStepAndStatus(params)
                        .then((response) => {
                            if (response.error !== undefined) {
                                console.log(response.error);
                            }
                        })
                        .finally(() => {
                            this.$emit('reload');
                        });
                }
            }
        },
        computed: {
            verifyFirst() {
                return this.isFirstStep == true ? this.$t("labelAnalyze") : this.$t("labelAdvance");
            }
        },
    };
</script>

<style scoped>

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-warning {
        background-color: #fef9c2 !important;
        color: #a65f00 !important;
    }

    .bg-danger {
        background-color: #ffedd4 !important;
        color: #ca3500 !important;
    }

    .bg-success {
        background-color: #d0fae5 !important;
        color: #007a55 !important;
    }
    .card {
        white-space: nowrap;
    }
</style>

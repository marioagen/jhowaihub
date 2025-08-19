<template>
        <div class="card">
            <div class="card-content">
                <div class="cover" v-if="progress < 100">
                    <div class="spinner-cover">
                        <LucideIcon icon="Loader" size="24" class="me-1 animate-spin" />
                    </div>
                    <div class="progress-content">
                        <div class="mb-2">{{ $t("labelprogress") }} <span class="float-end">{{ progress }}%</span></div>
                        <div class="progress">
                            <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" 
                                :aria-valuenow="progress" 
                                aria-valuemin="0" 
                                aria-valuemax="100" 
                                :style="{ width: (progress) + '%'}"></div>
                        </div>
                    </div>
                </div>
                <div class="card-body" :class="progress < 100 ? 'hide-card' : ''">
                    <p>{{ dataCard.name }}</p>
                    <div class="mb-2">
                        <LucideIcon icon="FileText" size="12" class="me-1" />
                        <small>{{ dataCard.description }}</small>
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
                    <div class="mb-2 footer">
                        <button class="btn btn-sm btn-primary float-end" @click="advanceStep" v-if="!isLastStep">
                            <span>{{ verifyFirst }}</span>
                            <LucideIcon icon="ChevronRight" size="16" class="me-1" />
                        </button>
                        <div class="badge" :style="badgeStyle(dataStep.status.color)">{{dataStep.status.name}}</div>
                    </div>
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
            },
            progress: {
                type: Number,
                required: true,
                default: 0,
            },
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

    .card-content {
        position: relative
    }

    .progress-content{
        width: 100%;
        z-index: 11;
        position: absolute;
        bottom: 0;
        padding: 15px;
    }
        .progress-content .progress {
            height: 10px;
        }

    .spinner-cover {
        position: absolute;
        inset: calc(.25rem * 0);
        align-items: center;
        display: flex;
        justify-content: center;
        z-index: 10;
        background-color: var(--color-card-content);
        opacity: 0.8;
    }

    .hide-card div, .hide-card p{
        color: transparent;
        height: 15px;
        background: linear-gradient(
            90deg,
            var(--skeleton-base) 25%,
            var(--skeleton-highlight) 37%,
            var(--skeleton-base) 63%
        );
        background-size: 400% 100%;
        animation: shimmer 1.4s ease infinite;
        border-radius: 8px;
    }

    @keyframes shimmer {
        0% {
            background-position: -400px 0;
        }
        100% {
            background-position: 400px 0;
        }
    }

    .hide-card .footer {
        display: none;
    }

    .animate-spin {
        animation: spin 1s linear infinite;
        color: var(--color-bg-icon-active);
    }

    @keyframes spin {
        100% {
            transform: rotate(360deg);
        }
    }
</style>

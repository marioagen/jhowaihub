<template>
    <div class="card clickable" @click="redirectToAnalyzer">
        <div class="card-content">
            <div class="cover" v-if="showLoading">
                <div class="spinner-cover">
                    <LucideIcon icon="Loader" :size="24" class="me-1 animate-spin" />
                </div>
                <div class="progress-content" v-if="showLoading">
                    <div class="mb-2">{{ $t("labelProcessing") }} <span class="float-end">{{ getProgressPercentage(dataCard.statusDocument) || 0 }}%</span></div>
                    <div class="progress">
                        <div class="progress-bar progress-bar-striped progress-bar-animated"
                             role="progressbar"
                             :aria-valuenow="getProgressPercentage(dataCard.statusDocument) || 0"
                             aria-valuemin="0"
                             aria-valuemax="100"
                             :style="{ width: (getProgressPercentage(dataCard.statusDocument) || 0) + '%' }">
                        </div>
                    </div>
                </div>
            </div>
            <div class="card-body" :class="showLoading ? 'hide-card' : ''">
                <p>{{ dataCard.name }}</p>
                <div class="mb-2">
                    <LucideIcon icon="FileText" :size="12" class="me-1" />
                    <small>{{ dataCard.description }}</small>
                </div>
                <div class="mb-2">
                    <LucideIcon icon="Calendar" :size="12" class="me-1" />
                    <small>{{ dataCard.created }}</small>
                </div>
                <hr>
                <div class="mb-2">
                    <LucideIcon icon="User" :size="12" class="me-1" />
                    <small>{{ dataCard.owner }}</small>
                </div>
                <div class="mb-2 d-flex justify-content-between align-items-center flex-wrap" v-if="!showLoading">
                    <div class="badge flex-shrink-1" :style="badgeStyle(dataStep.status.color)">
                        {{ dataStep.status.name }}
                    </div>
                    <button class="btn btn-sm btn-primary float-end" @click.stop="advanceStep" v-if="!isLastStep">
                        <span>{{ verifyFirst }}</span>
                        <LucideIcon icon="ChevronRight" :size="16" class="me-1" v-if="!isLoadingAnalysis" />
                        <div class="spinner-grow text-light" role="status"  v-if="isLoadingAnalysis"></div>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import CardsServices from "@/services/cards/CardsServices";

    export default {
        name: "CardComponent",
        emits: ["reload"],
        data: () => ({
            isLoadingAnalysis: false,
            statusProgress: null,
            signalrEventStatusChanged: "StatusChanged"
        }),
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
        },
        methods: {
            badgeStyle(color) {
                return {
                    '--cor-base': color,
                    color: 'var(--cor-base)',
                    backgroundColor: 'color-mix(in srgb, var(--cor-base) 30%, white)'
                };
            },
            async updateStatus() {
                if (!this.isLastStep) {
                    var params = {
                        CardId: this.dataCard.id,
                        NextStepOrder: this.dataStep.order + 1,
                        WorkflowId: this.dataStep.workflowId,
                    }
                    const response = await CardsServices.updateStepAndStatus(params);
                    if (response?.error !== undefined) {
                        this.$notify({
                            title: 'Error',
                            message: response.error,
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    }
                }
            },
            async advanceStep() {
               this.isLoadingAnalysis = true;
                    try {
                        await this.updateStatus();
                        if (this.isFirstStep) {
                            this.redirectToAnalyzer();
                        } else {
                            this.reloadList();
                        }

                        
                    } catch (e) {
                        this.$notify({
                            title: 'Error',
                            message: e.message || 'An error occurred while advancing the step.',
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    } finally {
                        this.isLoadingAnalysis = false;
                    }
                },
            getProgressPercentage(status) {
                switch (status) {
                    case 0:
                        return 0;
                    case 2:
                        return 50;
                    case 3:
                        return 100; 
                    default:
                        return 0; 
                }
            },
            redirectToAnalyzer() {
                if (!this.showLoading) {
                    this.$router.push({ name: 'Analyzer', params: { id: this.dataCard.documentId }, query: { page: this.backPage } });
                }
            },
            reloadList() {
                this.$emit('reload');
            },
        },
        computed: {
            verifyFirst() {
                return this.isFirstStep == true ? this.$t("labelAnalyze") : this.$t("labelAdvance");
            },
            showLoading() {
                return this.dataCard.statusDocument === 2 || this.dataCard.statusDocument === 0 || this.dataCard.statusDocument === 4;
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

    .card-body p,
    .card-body small {
        overflow-wrap: break-word;
        white-space: normal;
    }

    .card-body .badge {
        max-width: 60%;
        overflow-wrap: break-word;
        white-space: normal;
    }
    .overlay-loading {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(255,255,255,0.7);
        z-index: 9999;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 3rem;
        height: 3rem;
    }

    .clickable {
        cursor: pointer;
    }

    .spinner-grow{
        width: 1rem;
        height: 1rem;
        margin-left: 5px;
    }

</style>

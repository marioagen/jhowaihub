<template>
    <ModalComponent
        id="modalViewRejection"
        :isLoading="loading"
        ref="ModalViewRejection"
    >
        <template #header>
            <div class="modal-header border-0 pb-0">
                <div class="d-flex align-items-center">
                    <div class="bg-warning bg-opacity-10 p-2 rounded-circle me-3">
                        <i class="bi bi-info-circle text-warning fs-4"></i>
                    </div>
                    <div>
                        <h5 class="modal-title fw-bold">{{ $t("analyze.justification.title") }}</h5>
                        <small class="text-muted">{{ $t("analyze.justification.subtitle") }}</small>
                    </div>
                </div>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body py-4">
                <div
                    v-if="loading"
                    class="text-center py-3"
                >
                    <div
                        class="spinner-border text-primary"
                        role="status"
                    >
                        <span class="visually-hidden">{{ $t("common.loading") }}</span>
                    </div>
                </div>
                <div
                    v-else
                    class="rejection-list"
                    style="max-height: 400px; overflow-y: auto"
                >
                    <div
                        v-for="(rejection, index) in rejections"
                        :key="index"
                        class="card border mb-3"
                    >
                        <div class="card-body">
                            <h6
                                class="text-muted mb-2 upppercase"
                                style="font-size: 0.8rem"
                            >
                                {{ index + 1 }}. {{ $t("analyze.justification.reason") }}:
                            </h6>
                            <p class="mb-3">{{ rejection.justification }}</p>

                            <div
                                class="d-flex justify-content-between align-items-center mt-3 pt-3 border-top"
                            >
                                <div>
                                    <small class="text-muted d-block">
                                        {{ $t("analyze.justification.rejectedBy") }}:
                                    </small>
                                    <span class="fw-bold">{{ rejection.userName }}</span>
                                </div>
                                <div class="text-end">
                                    <small class="text-muted d-block">
                                        {{ $t("analyze.justification.date") }}:
                                    </small>
                                    <span class="fw-bold">{{ formatDate(rejection.date) }}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div
                        v-if="rejections.length === 0 && !loading"
                        class="text-center text-muted py-3"
                    >
                        {{ $t("analyze.justification.noJustificationsFound") }}
                    </div>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer border-0 pt-0 justify-content-center">
                <button
                    type="button"
                    class="btn btn-primary px-4"
                    @click="close"
                >
                    {{ $t("common.close") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import AnalysisRejectionServices from "@/services/documents/AnalysisRejectionServices";
    import dateHelper from "@/helpers/date";
    import LogService from "@/services/log/logService";

    export default {
        name: "DocumentViewRejectionModal",
        components: {
            ModalComponent,
        },
        data() {
            return {
                rejections: [],
                cardId: 0,
                loading: false,
            };
        },
        methods: {
            async fetchRejections() {
                this.loading = true;
                await AnalysisRejectionServices.findRejections(this.cardId)
                    .then((response) => {
                        if (response && !response.error) {
                            this.rejections = response;
                        } else {
                            this.rejections = [];
                        }
                    })
                    .catch((err) => {
                        LogService.showMessage("Error fetching rejections: " + err);
                    })
                    .finally(() => {
                        this.loading = false;
                    });
            },
            open(card) {
                this.cardId = card;
                this.rejections = [];
                this.fetchRejections();
                this.$refs.ModalViewRejection.open();
            },
            close() {
                this.$refs.ModalViewRejection.close();
                this.$emit("close");
            },
            formatDate(date) {
                if (!date) return "-";
                return dateHelper.formatDate(date);
            },
        },
    };
</script>
<style scoped>
    .modal-content {
        border-radius: 12px;
        border: none;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
    }
</style>

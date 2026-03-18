<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorDocumentFilters @filter="filterData" />
                    <AuditorDocumentSummary
                        ref="AuditorDocumentSummary"
                        :filters="filterParams"
                        @select-document="onDocumentSelection"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorDocumentDetail
                    :selected-document="selectedDocument"
                    :selected-document-workflows="selectedDocument?.workflows ?? []"
                    ref="AuditorDocumentDetail"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorDocumentFilters from "@/components/auditor/documents/AuditorDocumentFilters.vue";
    import AuditorDocumentSummary from "@/components/auditor/documents/AuditorDocumentSummary.vue";
    import AuditorDocumentDetail from "@/components/auditor/documents/AuditorDocumentDetail.vue";

    export default {
        name: "AuditorDocumentsSection",
        components: {
            AuditorDocumentFilters,
            AuditorDocumentSummary,
            AuditorDocumentDetail,
        },
        data() {
            return {
                filterParams: {
                    search: "",
                    statusId: "",
                },
                selectedDocument: null,
            };
        },
        methods: {
            filterData(filters) {
                this.filterParams = filters;
                this.$refs.AuditorDocumentSummary?.refreshWithCurrentFilters();
            },
            onDocumentSelection(document) {
                this.selectedDocument = document;
                this.$nextTick(() => {
                    this.$refs.AuditorDocumentDetail?.refreshWithCurrentDocument();
                });
            },
        },
    };
</script>
<style scoped>
    .auditor-summary-card,
    .auditor-detail-card {
        height: 70vh;
        display: flex;
        flex-direction: column;
        overflow: hidden;
    }
    .auditor-summary-card .auditor-summary-card-body,
    .auditor-detail-card > * {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }
</style>

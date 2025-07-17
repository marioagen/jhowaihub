<template>
    <main>
        <div class="container-fluid mt-4">
            <div class="custom-padding">
                <div class="row">
                    <!-- Component Breadcrumb -->
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <div class="row">
                    <!-- Component PromptView -->
                    <prompt-view
                        :hashDocument="hashDocument"
                        :historyListOrder="historyListOrder"
                        @showHistory="showHistory"
                        @unshiftHistoryList="unshiftHistoryList"
                        @pushHistoryList="pushHistoryList"
                        @showAlertToast="showAlertToast"
                        @clearMyInterval="clearMyInterval"
                        v-if="!isExpandedHistory"
                    />
                    <!-- Component DocView -->
                    <doc-view @showNormalize="normalize" id="docView" />
                    <div :class="!isExpandedHistory ? 'col-md-3' : 'col-md-6'" id="docHistory">
                        <!--Component HistoryView-->
                        <history-view
                            :dataShowHistory="dataShowHistory"
                            :dataIsExpandedHistory="isExpandedHistory"
                            :dataUnshiftHistoryList="dataUnshiftHistoryList"
                            :dataPushHistoryList="dataPushHistoryList"
                            @expandHistory="expandHistory"
                            @showAlertToast="showAlertToast"
                            @clearMyInterval="clearMyInterval"
                            @updateHistoryListOrder="updateHistoryListOrder"
                        />
                    </div>
                </div>
            </div>
            <div v-if="isExpandedHistory" style="position: absolute; top: 50%">
                <a class="btn btn-light btn-sm shadow" :title="$t('labelQuestionnaireAndAi')" @click="expandHistory">
                    <img src="./../../../assets/img/prompt.png" />
                </a>
            </div>
        </div>
        <!-- Component ToastAlert -->
        <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
        <NormalizeIndex :docData="dataView" :isReprocessing="isReprocessing" v-if="showLoading"></NormalizeIndex>
    </main>
</template>

<script>
    import NavBar from "@/components/common/nav-bar";
    import Breadcrumb from "@/components/common/breadcrumb";
    import PromptView from "@/components/pages/analyzer/prompt-view";
    import DocView from "@/components/pages/analyzer/doc-view";
    import HistoryView from "@/components/pages/analyzer/history-view";
    import ToastAlert from "@/components/common/toast-alert";
    import api from "@/services/api";
    import NormalizeIndex from "@/components/pages/normalize/loading";

    export default {
        name: "AnalyzerIndex",
        data() {
            return {
                crumbsData: [],
                sidebarData: "DocumentList",
                idAnalyzer: this.$route.params.id,
                backPage: this.$route.query.page,
                hashDocument: "",
                isExpandedHistory: false,
                historyListOrder: "desc",
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                myInterval: null,
                dataShowHistory: true,
                dataUnshiftHistoryList: {},
                dataPushHistoryList: {},
                dataView: {
                    Id: parseInt(this.idAnalyzer),
                    Embeddings_model_name: "",
                },
                isReprocessing: true,
                showLoading: false,
            };
        },
        components: {
            NavBar,
            Breadcrumb,
            PromptView,
            DocView,
            HistoryView,
            ToastAlert,
            NormalizeIndex,
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
            },
        },
        methods: {
            normalize: function (dataView, isReprocessing) {
                this.dataView = dataView;
                this.isReprocessing = isReprocessing;
                this.showLoading = true;
            },
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t("labelDocuments"), link: { to: "DocumentList" } },
                    { crumb: this.$t("labelListing"), link: { to: "DocumentList", queryPage: this.$route.query.page } },
                    { crumb: this.$t("labelConsult"), link: { to: "Analyzer", queryPage: this.$route.query.page } },
                ];
            },
            expandHistory: function () {
                this.isExpandedHistory = !this.isExpandedHistory;
            },
            updateHistoryListOrder: function (data) {
                this.historyListOrder = data.value;
            },
            showHistory: function () {
                this.dataShowHistory = !this.dataShowHistory;
            },
            unshiftHistoryList: function (data) {
                this.dataUnshiftHistoryList = data;
            },
            pushHistoryList: function (data) {
                this.dataPushHistoryList = data;
            },
            showAlertToast: function (data) {
                this.alertToast(data.msg, data.color);
            },
            getDataDocument: function () {
                let self = this;
                api.get("/Document/Analyze/" + this.idAnalyzer)
                    .then(function (result) {
                        // Handle success
                        self.hashDocument = result.data.referenceFile;
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                let self = this;
                this.myInterval = setInterval(function () {
                    self.toastMessage = "";
                    self.toastColor = "";
                    self.toastShow = false;
                    clearInterval(self.myInterval);
                }, 3000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval);
                this.myInterval = null;
            },
        },
        computed: {},
        created() {
            this.setCrumbsData();
            this.getDataDocument();
        },
        mounted() {},
        unmounted() {},
    };
</script>

<style scoped>
    .container-fluid {
        padding: 0 13px;
    }

    @media (min-width: 320px) and (max-width: 767px) {
        #docView {
            display: none;
        }
        #docHistory {
            display: none;
        }
    }
</style>

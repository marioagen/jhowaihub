<template>
    <div class="div-row background-white shadow">
        <div class="row" style="margin-top: 35px !important">
            <div class="col mt-2">
                <h6 class="mt-0 text-primary">
                    <i
                        class="fas fa-compress"
                        :title="$t('documents.reduceHistory')"
                        @click="expandHistory"
                        v-show="isExpandedHistory"
                        style="margin-left: 1%; cursor: pointer"
                    ></i>
                    <i
                        class="fas fa-expand text-primary"
                        :title="$t('documents.expandHistory')"
                        @click="expandHistory"
                        v-show="!isExpandedHistory"
                        style="margin-left: 4%; cursor: pointer"
                    ></i>
                    {{ $t("documents.historic") }}
                    <a style="margin-left: 1%" @click="expandSort">
                        <i class="fas fa-sort" style="color: #0073e6; cursor: pointer" :title="$t('common.order')"></i>
                    </a>
                    <container class="flex-shrink">
                        <span
                            class="badge rounded-pill bg-primary badge-custom"
                            style="margin-left: 1.5%; cursor: pointer"
                            :title="$t('dashboard.downloadCsv')"
                            @click="downloadHistory"
                            v-if="historyList.length > 0"
                        >
                            <i class="fas fa-cloud-download-alt"></i>
                            {{ historyList.length }}
                        </span>
                        <span
                            class="badge rounded-pill bg-danger badge-custom"
                            style="margin-left: 1.5%; cursor: pointer"
                            :title="$t('documents.deleteHistory')"
                            @click="confirmationDialog(idAnalyzer)"
                            v-if="historyList.length > 0"
                        >
                            <i class="fas fa-trash" style="font-size: 0.9em"></i>
                        </span>
                    </container>
                    <span class="dropdown-menu" v-show="isExpandedSort" style="display: block">
                        <a class="dropdown-item" @click="sortHistoryListAsc">
                            <i
                                class="fas fa-check"
                                style="color: #0073e6"
                                title="check"
                                v-show="historyListOrder == 'desc'"
                            ></i>
                            <i
                                class="far fa-circle"
                                style="color: #0073e6"
                                title="circle"
                                v-show="historyListOrder == 'asc'"
                            ></i>
                            &nbsp;{{ $t("documents.mostRecent") }}
                        </a>
                        <a class="dropdown-item" @click="sortHistoryListDesc">
                            <i
                                class="fas fa-check"
                                style="color: #0073e6"
                                title="check"
                                v-show="historyListOrder == 'asc'"
                            ></i>
                            <i
                                class="far fa-circle"
                                style="color: #0073e6"
                                title="circle"
                                v-show="historyListOrder == 'desc'"
                            ></i>
                            &nbsp;{{ $t("documents.mostOlder") }}
                        </a>
                    </span>
                </h6>
            </div>
        </div>
        <div :class="historyList.length == 0 ? 'row mb-2' : 'row'" v-if="loadingHistory">
            <span>
                &nbsp;&nbsp;&nbsp;
                <i class="fas fa-sync-alt fa-spin"></i>
                &nbsp;{{ $t("common.loading") }}..
            </span>
        </div>
        <div class="row mb-2" v-if="!loadingHistory && historyList.length == 0">
            <span>&nbsp;&nbsp;&nbsp;{{ $t("documents.queryWithoutHistory") }}.</span>
        </div>
        <div class="div-row-overflow" ref="historyDiv">
            <div class="row" v-for="(history, index) in historyList" :key="index">
                <div class="col-md-12">
                    <div class="div-card">
                        <div class="card shadow">
                            <div class="card-body">
                                <h5 class="card-title" style="margin-bottom: 8px">{{ history.input }}</h5>
                                <div class="card-subtitle mb-1 text-primary">
                                    <div style="float: right; margin-left: 2%">
                                        <a @click="copyToClipboard(history.output)">
                                            <i
                                                class="fas fa-clone text-primary"
                                                :title="$t('common.copy')"
                                                style="cursor: pointer"
                                            ></i>
                                        </a>
                                    </div>
                                    <div style="float: right">
                                        <a @click="changeTextArea(index)">
                                            <i
                                                class="fas fa-pen text-primary"
                                                :title="$t('common.edit')"
                                                style="cursor: pointer"
                                            ></i>
                                        </a>
                                    </div>
                                </div>
                                <textarea
                                    class="form-control mb-1 blue-textarea"
                                    rows="5"
                                    :id="index"
                                    v-model="history.output"
                                    readonly
                                ></textarea>
                                <div v-if="history.isEditing == true">
                                    <div style="float: right; margin-left: 2%">
                                        <a>
                                            <i
                                                class="fa fa-check text-primary"
                                                @click="editHistory(index)"
                                                :title="$t('common.confirm')"
                                            ></i>
                                        </a>
                                    </div>
                                    <div style="float: right">
                                        <a>
                                            <i
                                                class="fa fa-times text-secondary"
                                                @click="stopEditing(index)"
                                                :title="$t('common.cancel')"
                                            ></i>
                                        </a>
                                    </div>
                                </div>
                                <div v-if="history.isEdited == true" class="text-primary edited">
                                    {{ $t("common.edited") }}
                                </div>
                                <div v-if="history.isEditedFail == true" class="text-danger fail">
                                    {{ $t("common.editedFail") }}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- Component ModalAlert -->
    <modal-alert
        v-if="modalAlertShow"
        :type="'Confirm'"
        :entity="modalEntity"
        :alertTitle="$t('documents.youAreAboutToDeleteDocumentQuery')"
        :alertMessage="$t('common.thisActionCannotBeUndone')"
        :okLabel="$t('common.confirm')"
        :cancelLabel="$t('common.cancel')"
        @open="deleteHistory"
        @close="closeModal"
    />
</template>

<script>
    import date from "@/helpers/date";
    import api from "@/services/api";
    import ModalAlert from "@/components/pages/analyzer/modal-alert";

    export default {
        name: "HistoryView",
        emits: ["expandHistory", "showAlertToast", "clearMyInterval", "updateHistoryListOrder"],
        props: {
            dataShowHistory: {
                required: true,
                type: Boolean,
                default: true,
            },
            dataIsExpandedHistory: {
                required: true,
                type: Boolean,
                default: false,
            },
            dataUnshiftHistoryList: {
                required: true,
                type: Object,
                default: {},
            },
            dataPushHistoryList: {
                required: true,
                type: Object,
                default: {},
            },
        },
        data() {
            return {
                idAnalyzer: this.$route.params.id,
                historyList: [],
                loadingHistory: false,
                isExpandedHistory: this.dataIsExpandedHistory,
                isExpandedSort: false,
                historyListOrder: "asc",
                modalAlertShow: false,
                modalEntity: {},
                oldOutput: "",
                isEditing: false,
                isEditedFail: false,
            };
        },
        components: {
            ModalAlert,
        },
        watch: {
            dataShowHistory: {
                handler(val, oldVal) {
                    this.showHistory();
                },
                deep: true,
            },
            dataIsExpandedHistory: {
                handler(val, oldVal) {
                    this.isExpandedHistory = val;
                },
                deep: true,
            },
            dataUnshiftHistoryList: {
                handler(val, oldVal) {
                    this.unshiftHistoryList(val);
                },
                deep: true,
            },
            dataPushHistoryList: {
                handler(val, oldVal) {
                    this.pushHistoryList(val);
                },
                deep: true,
            },
        },
        methods: {
            showHistory: function () {
                this.loadingHistory = true;
                let self = this;
                api.get("/Document/History/" + this.idAnalyzer)
                    .then(function (response) {
                        // Handle success
                        setTimeout(function () {
                            self.loadingHistory = false;
                            self.historyList = response.data.value.reverse();
                            const result = self.historyList.reduce((acc, cur, index) => {
                                acc.push({ ...cur, isEditedFail: false, isEditing: false });
                                return acc;
                            }, []);
                            self.historyList = result;
                        }, 1000);
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.historyList = [];
                        self.loadingHistory = false;
                        self.clearMyInterval();
                        self.alertToast(self.$t("analyze.failedToLoadHistory"), "toast-danger");
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            downloadHistory: function () {
                var csv = "Input;Output";
                for (var i = 0; i < this.historyList.length; i++) {
                    csv +=
                        "\n" +
                        this.historyList[i].input +
                        ";" +
                        this.historyList[i].output.replaceAll(/[\r\n\s]+/g, " ");
                    if (i == this.historyList.length - 1) {
                        // Download forced - Begin
                        var fileURL = window.URL.createObjectURL(
                            new Blob(["\ufeff" + csv], { type: "text/csv;charset=utf-8" })
                        );
                        var fileLink = document.createElement("a");
                        fileLink.href = fileURL;
                        fileLink.setAttribute(
                            "download",
                            this.$t("documents.historic") + " - " + this.dateFormat(Date.now()) + ".csv"
                        );
                        document.body.appendChild(fileLink);
                        fileLink.click();
                        // Download forced - End
                        this.clearMyInterval();
                        this.alertToast(this.$t("dashboard.downloadSuccessfully"), "toast-success");
                    }
                }
            },
            confirmationDialog: function (item) {
                this.modalEntity = { id: item };
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            deleteHistory: function (id) {
                let self = this;
                api.delete("/Document/History/" + id)
                    .then(function (response) {
                        // Handle success
                        self.closeModal();
                        setTimeout(() => self.showHistory(), 100);
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
            closeModal: function () {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            expandHistory: function () {
                this.isExpandedHistory = !this.isExpandedHistory;
                this.$emit("expandHistory");
            },
            unshiftHistoryList(data) {
                this.historyList.unshift({
                    input: data.input,
                    output: data.output,
                });

                this.$nextTick(() => {
                    if (this.$refs.historyDiv) {
                    this.$refs.historyDiv.scrollTo({
                        top: 0,
                        behavior: "smooth",
                    });
                    }
                });
            },
            pushHistoryList: function (data) {
                this.historyList.push({
                    input: data.input,
                    output: data.output,
                });
                let self = this;
                setTimeout(function () {
                    self.moveSidebar();
                }, 100);
            },
            moveSidebar() {
                this.$nextTick(() => {
                    const el = this.$refs.historyDiv;
                    if (el) {
                    el.scrollTo({
                        top: el.scrollHeight,
                        behavior: "smooth",
                    });
                    }
                });
            },
            copyToClipboard: function (content) {
                navigator.clipboard.writeText(content);
                this.clearMyInterval();
                this.alertToast(this.$t("common.textCopiedToClipboard"), "toast-primary");
            },
            changeTextArea: function (id) {
                this.historyList[id].isEditing = true;
                var textarea = document.getElementById(id);
                this.oldOutput = textarea.value;
                textarea.removeAttribute("readonly");
            },
            editHistory: function (id) {
                var textarea = document.getElementById(id);
                textarea.setAttribute("readonly", "readonly");
                var textUpdated = textarea.value;

                self = this;
                var paramsData = {
                    idDocument: this.idAnalyzer,
                    oldOutput: this.oldOutput,
                    updatedOutput: textUpdated,
                };
                api.put("/Document", paramsData)
                    .then(function (response) {
                        // Handle success
                        self.oldOutput = "";
                        self.historyList[id].isEditing = false;
                    })
                    .catch(function (e) {
                        // Handle error
                        self.historyList[id].output = self.oldOutput;
                        console.log(e);
                        self.historyList[id].isEditedFail = true;
                        textarea.classList.remove("blue-textarea");
                        textarea.classList.add("red-textarea");
                        setTimeout(() => {
                            self.historyList[id].isEditedFail = false;
                            textarea.classList.remove("red-textarea");
                            textarea.classList.add("blue-textarea");
                        }, 5000);
                    })
                    .finally(function () {
                        // Always executed
                        self.showHistory();
                        console.log("Finished request.");
                    });
            },
            stopEditing: function (id) {
                var textarea = document.getElementById(id);
                this.historyList[id].output = this.oldOutput;
                textarea.setAttribute("readonly", "readonly");
                this.oldOutput = "";
                this.historyList[id].isEditing = false;
            },
            alertToast: function (msg, color) {
                this.$emit("showAlertToast", { msg: msg, color: color });
            },
            clearMyInterval: function () {
                this.$emit("clearMyInterval");
            },
            expandSort: function () {
                this.isExpandedSort = !this.isExpandedSort;
            },
            sortHistoryListAsc: function () {
                if (this.historyListOrder == "desc") {
                    this.historyListOrder = "asc";
                    this.$emit("updateHistoryListOrder", { value: "asc" });
                    this.historyList.reverse();
                }
                this.isExpandedSort = false;
            },
            sortHistoryListDesc: function () {
                if (this.historyListOrder == "asc") {
                    this.historyListOrder = "desc";
                    this.$emit("updateHistoryListOrder", { value: "desc" });
                    this.historyList.reverse();
                }
                this.isExpandedSort = false;
            },
            dateFormat: function (str) {
                return date.formatDate(str);
            },
        },
        computed: {},
        created() {
            this.showHistory();
        },
        mounted() {},
        unmounted() {},
    };
</script>

<style scoped>
    .fas,
    .far {
        font-weight: 900 !important;
    }

    .text-primary {
        color: #47aaff !important;
    }

    .div-row {
        border-radius: 6px !important;
    }

    .div-row-overflow {
        max-height: calc(100vh - 175px) !important;
        overflow: hidden;
        overflow-y: auto !important;
        overflow-x: none !important;
    }

    .div-card {
        padding: 10px;
    }

    .card-body {
        padding: 0.5rem 0.8rem !important;
    }

    .card-title {
        font-weight: normal !important;
        font-size: 1em !important;
    }

    .card-subtitle {
        font-weight: normal !important;
        font-size: 0.7em !important;
    }

    .blue-textarea {
        border-color: #0073e6 !important;
    }

    .red-textarea {
        border-color: darkred !important;
    }
    @media (min-width: 768px) and (max-width: 1023px) {
        .flex-shrink {
            justify-content: center;
            display: flex;
        }
    }
</style>

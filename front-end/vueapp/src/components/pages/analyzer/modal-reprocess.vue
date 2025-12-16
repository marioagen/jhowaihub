<template>
    <div class="modal fade show" id="exampleModalForm" tabindex="-1" aria-labelledby="exampleModalLabel">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">{{ $t("common.reprocess") }}</h5>
                    <button
                        type="button"
                        class="btn-close"
                        :title="$t('common.close')"
                        data-bs-dismiss="modal"
                        aria-label="Close"
                        @click="close"
                    ></button>
                </div>
                <div class="modal-body prevent-text-select">
                    <div>
                        <form @submit.prevent="save">
                            <div class="mb-3">
                                <label class="form-label">{{ $t("common.model") }}</label>
                                <select class="form-select" v-model="modelSelected">
                                    <option value="">{{ $t("common.select") }}</option>
                                    <option value="text-embedding-3-large">text-embedding-3-large</option>
                                </select>
                            </div>
                            <button
                                type="submit"
                                class="btn btn-primary m-2"
                                :title="$t('common.save')"
                                style="float: right"
                            >
                                {{ $t("common.save") }}
                            </button>
                            <a
                                class="btn btn-light m-2 btn-custom-cancel"
                                :title="$t('common.cancel')"
                                style="float: right"
                                @click="close"
                            >
                                {{ $t("common.cancel") }}
                            </a>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ModalReprocess",
        data() {
            return {
                modelSelected: "",
            };
        },
        methods: {
            close: function () {
                this.$emit("close");
            },
            save: function (e) {
                if (this.modelSelected != "") {
                    this.$emit("formatData", this.modelSelected);
                    this.close();
                }
            },
        },
    };
</script>
<style scoped>
    .show {
        display: block;
        padding-right: 17px;
    }

    .prevent-text-select {
        -webkit-user-select: none; /* Safari */
        -ms-user-select: none; /* IE 10 and IE 11 */
        user-select: none; /* Standard syntax */
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }
</style>

<template>
    <div class="modal fade show" id="exampleModalAlert" tabindex="-1" aria-labelledby="exampleModalLabel">
        <div
            :class="
                type === 'Confirm'
                    ? `modal-dialog modal-dialog-centered`
                    : `modal-dialog modal-sm modal-dialog-centered`
            "
        >
            <div class="modal-content">
                <div class="modal-body prevent-text-select">
                    <div>
                        <div class="row">
                            <div class="col content-center" style="padding: 14px">
                                <img src="./../../../assets/img/icon-alert.png" v-if="type === 'Error'" />
                                <img src="./../../../assets/img/icon-trash.png" v-if="type === 'Confirm'" />
                                <img src="./../../../assets/img/icon-warning.png" v-if="type === 'Warning'" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col content-center">
                                <h4 class="mb-3" style="text-align: center" v-html="alertTitle"></h4>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col content-center mb-2">
                                <span class="span-muted" v-html="alertMessage"></span>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col content-center">
                                <div class="spinner-border text-primary" role="status" v-if="isLoading"></div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer" style="justify-content: center">
                    <div class="mb-3" v-if="type === 'Error' || type === 'Warning'">
                        <button type="submit" class="btn btn-primary" @click="close">{{ okLabel }}</button>
                    </div>
                    <div class="mb-3" v-if="type === 'Confirm'">
                        <button type="submit" class="btn btn-light btn-custom-cancel" @click="close">
                            {{ cancelLabel }}
                        </button>
                        &nbsp;
                        <button type="submit" class="btn btn-primary" @click="open">{{ okLabel }}</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ModalAlert",
        props: {
            type: {
                required: true,
                type: String,
                default: "",
            },
            entity: {
                required: false,
                type: Object,
                default: {},
            },
            alertTitle: {
                required: false,
                type: String,
                default: "",
            },
            alertMessage: {
                required: false,
                type: String,
                default: "",
            },
            okLabel: {
                required: false,
                type: String,
                default: "",
            },
            cancelLabel: {
                required: false,
                type: String,
                default: "",
            },
        },
        data() {
            return {
                isLoading: false,
            };
        },
        methods: {
            open: function () {
                this.isLoading = true;
                this.$emit("open", this.entity.id);
            },
            close: function () {
                this.$emit("close");
            },
        },
    };
</script>
<style scoped>
    .show {
        display: block;
        padding-right: 17px;
    }

    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    .prevent-text-select {
        -webkit-user-select: none;
        /* Safari */
        -ms-user-select: none;
        /* IE 10 and IE 11 */
        user-select: none;
        /* Standard syntax */
    }

    .modal-footer {
        border-top: none;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .span-muted {
        text-align: center;
        line-height: initial;
        width: 90%;
    }

    .modal {
        z-index: 9999;
    }
</style>

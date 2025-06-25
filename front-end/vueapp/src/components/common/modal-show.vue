<template>
    <div class="modal fade show" id="exampleModalForm" tabindex="-1" aria-labelledby="exampleModalLabel">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">{{label}}</h5>
                    <button type="button" class="btn-close" :title="$t('labelClose')" data-bs-dismiss="modal" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body prevent-text-select">
                    <div>
                        <form>
                            <div class="mb-3">
                                <textarea type="text" class="form-control" v-model="form.name" disabled rows="5" v-if="this.form.name != ''"></textarea>
                                <textarea type="text" class="form-control" v-model="form.desc" disabled rows="5" v-else></textarea>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ModalShow",
        props: {
            dataShow: {
                required: true,
                type: Object,
                default: {}
            },
        },
        data() {
            return {
                form: {
                    desc: this.dataShow.description ? this.dataShow.description : '',
                    name: this.dataShow.name || this.dataShow.title ? this.dataShow.name || this.dataShow.title : '',
                },
            }
        },
        methods: {
            close: function () {
                this.$emit('close');
            },
        },
        computed:
        {
            label() {
                return this.form.name != '' ? this.$t('labelName') : this.$t('labelDescription');
            },
        },
    }
</script>
<style scoped>
    .show {
        display: block;
        padding-right: 17px;
    }

    .prevent-text-select {
        -webkit-user-select: none;
        -ms-user-select: none;
        user-select: none;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }
</style>

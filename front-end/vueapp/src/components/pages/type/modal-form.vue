<template>
    <div class="modal fade show" id="exampleModalForm" tabindex="-1" aria-labelledby="exampleModalLabel">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel" v-if="form.id > 0">{{ $t('labelEditType') }}</h5>
                    <h5 class="modal-title" id="exampleModalLabel" v-else>{{ $t('labelNewType') }}</h5>
                    <button type="button" class="btn-close" :title="$t('labelClose')" data-bs-dismiss="modal" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body prevent-text-select">
                    <div>
                        <form @submit="save">
                            <div class="mb-3">
                                <label class="form-label" for="nameId">{{ $t('labelName') }}</label>
                                <input type="text" class="form-control" id="nameId" 
                                v-validate ref="nameInpt" autocomplete="off" v-model="form.name" required>
                            </div>
                            <!-- Create button -->
                            <button type="submit" class="btn btn-primary m-2" :title="$t('labelCreate')" style="float:right" v-if="form.id === 0">{{ $t('labelCreate') }}</button>
                            <!-- Save button -->
                            <button type="submit" class="btn btn-primary m-2" :title="$t('labelSave')" style="float:right" v-else>{{ $t('labelSave') }}</button>
                            <a  class="btn btn-light m-2 btn-custom-cancel" :title="$t('labelCancel')" style="float:right" @click="close">{{ $t('labelCancel') }}</a>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ModalForm",
        directives: {
            validate: {
                inserted: function (el, binding) {
                    el.addEventListener('input', function () {
                        el.setCustomValidity('');
                        if (!el.checkValidity()) {
                            el.reportValidity();
                        }
                    });

                    el.addEventListener('invalid', function (event) {
                        event.preventDefault();
                        if (el.validity.valueMissing) {
                            el.setCustomValidity(this.$t('labelFillInThisField'));
                        }
                        el.reportValidity();
                    });
                }
            }
        },
        props: {
            dataEditing: {
                required: true,
                type: Object,
                default: {}
            },
        },
        data() {
            return {
                form: {
                    id: this.dataEditing.id ? this.dataEditing.id : 0,
                    name: this.dataEditing.name ? this.dataEditing.name : '',
                },
            }
        },
        methods: {
            save: function (e) {
                e.preventDefault();
                if (this.form.id != 0) { // Edit
                    this.$emit('openEdit', this.form);
                } else { // Create
                    this.$emit('openAdd', this.form.name);
                }
            },
            close: function () {
                this.$emit('close');
            },
        },
        mounted() {
            this.$refs.nameInpt.focus();
        },
    }
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

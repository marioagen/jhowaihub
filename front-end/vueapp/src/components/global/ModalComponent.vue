<template>
    <div :id="id" :aria-labelledby="`${id}-label`" class="modal fade" tabindex="-1" aria-hidden="true" ref="modalEl">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <slot name="header">
                    <div class="modal-header">
                        <h5 class="modal-title" :id="`${id}-label`">
                            {{ $t(title) }}
                        </h5>
                        <button
                            type="button"
                            class="btn-close"
                            data-bs-dismiss="modal"
                            aria-label="Close"
                            :disabled="isLoading"
                        />
                    </div>
                </slot>

                <slot name="body">
                    <div class="modal-body">
                        <slot />
                    </div>
                </slot>

                <slot name="footer">
                    <div class="modal-footer justify-content-center">
                        <button
                            type="button"
                            class="btn btn-outline-secondary"
                            data-bs-dismiss="modal"
                            :disabled="isLoading"
                            @click="$emit('cancel')"
                        >
                            {{ $t(cancelText) }}
                        </button>

                        <button type="button" class="btn btn-primary" :disabled="isLoading" @click="$emit('save')">
                            <div style="min-width: 80px" class="text-center">
                                <span v-if="isLoading" class="spinner-grow spinner-grow-sm" role="status"></span>
                                <span v-else>{{ $t(saveText) }}</span>
                            </div>
                        </button>
                    </div>
                </slot>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ModalComponent",
        props: {
            id: {
                type: String,
                required: true,
            },
            isLoading: {
                type: Boolean,
                default: false,
            },
            title: {
                type: String,
                required: false,
                default: "common.new",
            },
            cancelText: {
                type: String,
                required: false,
                default: "common.cancel",
            },
            saveText: {
                type: String,
                required: false,
                default: "common.save",
            },
        },
        mounted() {
            this.modalInstance = new window.bootstrap.Modal(this.$refs.modalEl, {
                backdrop: "static",
                keyboard: false,
            });
        },
        methods: {
            open() {
                this.modalInstance?.show();
            },
            close() {
                this.modalInstance?.hide();
            },
        },
    };
</script>

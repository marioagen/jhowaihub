<template>
    <div
        class="modal fade"
        tabindex="-1"
        aria-hidden="true"
        ref="modalEl"
        :aria-labelledby="`${id}-label`"
        :id="id"
    >
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div
                    class="modal-header d-flex flex-column align-items-center justify-content-center pt-4"
                >
                    <button
                        type="button"
                        class="btn-close position-absolute top-0 end-0 m-3"
                        data-bs-dismiss="modal"
                        aria-label="Close"
                        :disabled="isLoading"
                        @click="$emit('cancel')"
                    />
                    <div class="mb-3">
                        <div
                            class="rounded-circle d-flex align-items-center justify-content-center"
                            :class="`bg-${iconVariant}-subtle text-${iconVariant}`"
                            style="width: 64px; height: 64px"
                        >
                            <LucideIcon
                                :icon="iconeName"
                                :size="32"
                            />
                        </div>
                    </div>
                    <h5
                        class="modal-title fw-bold text-center"
                        :id="`${id}-label`"
                    >
                        {{ $t(title) }}
                    </h5>
                </div>

                <div class="modal-body text-center px-4 pt-4 pb-0 text-muted">
                    {{ displayMessage }}
                </div>

                <div class="px-4 mt-4">
                    <input
                        type="text"
                        class="form-control"
                        :placeholder="placeholder"
                        v-model="inputValue"
                        :disabled="isLoading"
                        @keyup.enter="handleEnter"
                    />
                </div>

                <div class="d-flex justify-content-end mx-4 my-4 gap-3">
                    <button
                        type="button"
                        class="btn btn-outline-primary btn-table table-btn mx-4"
                        data-bs-dismiss="modal"
                        :disabled="isLoading"
                        @click="$emit('cancel')"
                    >
                        {{ $t(cancelText) }}
                    </button>
                    <button
                        type="button"
                        :class="`btn btn-${confirmVariant}`"
                        :disabled="isLoading || !isValid"
                        @click="$emit('confirm')"
                    >
                        <div
                            style="min-width: 80px"
                            class="text-center"
                        >
                            <LucideIcon
                                v-if="!isLoading"
                                :icon="iconeName"
                                :size="18"
                                class="me-1"
                            />
                            <span
                                v-if="isLoading"
                                class="spinner-grow spinner-grow-sm"
                                role="status"
                            ></span>
                            <span v-else>{{ $t(confirmText) }}</span>
                        </div>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ConfirmModalValidationInput",
        props: {
            id: {
                type: String,
                required: true,
            },
            isLoading: {
                type: Boolean,
                required: true,
            },
            title: {
                type: String,
                required: true,
            },
            message: {
                type: String,
                required: false,
                default: "",
            },
            messageKey: {
                type: String,
                required: false,
                default: "",
            },
            messageParams: {
                type: Object,
                required: false,
                default: () => ({}),
            },
            cancelText: {
                type: String,
                default: "Cancelar",
            },
            confirmText: {
                type: String,
                default: "Confirmar",
            },
            placeholder: {
                type: String,
                default: "Digite para confirmar",
            },
            validationText: {
                type: String,
                required: true,
            },
            confirmVariant: {
                type: String,
                required: false,
                default: "danger",
            },
            iconeName: {
                type: String,
                required: false,
                default: "Trash2",
            },
            iconVariant: {
                type: String,
                required: false,
                default: "danger",
            },
        },
        data() {
            return {
                inputValue: "",
            };
        },
        computed: {
            displayMessage() {
                if (this.messageKey) {
                    return this.$t(this.messageKey, this.messageParams);
                }
                return this.message;
            },
            isValid() {
                return this.inputValue === this.validationText;
            },
        },
        mounted() {
            this.modalInstance = new window.bootstrap.Modal(this.$refs.modalEl, {
                backdrop: "static",
                keyboard: false,
            });
            this.$refs.modalEl.addEventListener("hidden.bs.modal", this.resetInput);
        },
        beforeUnmount() {
            this.$refs.modalEl.removeEventListener("hidden.bs.modal", this.resetInput);
        },
        methods: {
            open() {
                this.resetInput();
                this.modalInstance?.show();
            },
            close() {
                this.modalInstance?.hide();
                this.resetInput();
            },
            resetInput() {
                this.inputValue = "";
            },
            handleEnter() {
                if (this.isValid && !this.isLoading) {
                    this.$emit("confirm");
                }
            },
        },
    };
</script>

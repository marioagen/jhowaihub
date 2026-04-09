<template>
    <ModalComponent
        id="modalAnonymization"
        :isLoading="loading"
        @save="confirm"
        ref="ModalAnonymization"
    >
        <template #header>
            <div class="modal-header border-0">
                <div>
                    <h5 class="modal-title fw-bold">
                        <i class="fas fa-shield-alt text-primary me-2"></i>
                        {{ $t("anonymization.title") }}
                    </h5>
                    <small class="text-muted d-block">
                        {{ $t("anonymization.subtitle") }}
                    </small>
                </div>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body">
                <div class="mb-3">
                    <label
                        for="anonymizationType"
                        class="form-label"
                    >
                        {{ $t("anonymization.type") }}
                        <span class="text-muted">({{ $t("common.optional") }})</span>
                    </label>
                    <select
                        v-model="anonymizationType"
                        class="form-select"
                        id="anonymizationType"
                    >
                        <option value="">{{ $t("anonymization.selectType") }}</option>
                        <option
                            v-for="type in anonymizationTypes"
                            :key="type.value"
                            :value="type.value"
                        >
                            {{ type.label }}
                        </option>
                    </select>
                </div>
                <div class="mb-3">
                    <label
                        for="promptId"
                        class="form-label"
                    >
                        {{ $t("anonymization.prompt") }}
                        <span class="text-muted">({{ $t("common.optional") }})</span>
                    </label>
                    <select
                        v-model="promptId"
                        class="form-select"
                        id="promptId"
                        :disabled="loadingPrompts"
                    >
                        <option value="">{{ $t("anonymization.selectPrompt") }}</option>
                        <option
                            v-for="prompt in prompts"
                            :key="prompt.id"
                            :value="prompt.id"
                        >
                            {{ prompt.name }}
                        </option>
                    </select>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer justify-content-between">
                <button
                    type="button"
                    class="btn btn-light"
                    @click="close"
                >
                    {{ $t("anonymization.cancel") }}
                </button>
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="confirm"
                    :disabled="loading"
                >
                    {{ $t("anonymization.confirm") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import PromptsService from "@/services/prompts/PromptsService";
    import AnonymizationServices from "@/services/anonymization/AnonymizationServices";
    import LogService from "@/services/log/logService";
    import store from "@/store";

    export default {
        name: "AnonymizationModal",
        components: {
            ModalComponent,
        },
        props: {
            documentId: {
                type: [Number, String],
                required: true,
            },
        },
        data() {
            return {
                prompts: [],
                loading: false,
                loadingPrompts: false,
                anonymizationType: "",
                promptId: "",
                anonymizationTypes: [
                    {
                        value: 1,
                        label: this.$t("anonymization.types.partialMasking"),
                    },
                    {
                        value: 2,
                        label: this.$t("anonymization.types.totalMasking"),
                    },
                    {
                        value: 3,
                        label: this.$t("anonymization.types.replaceWithInitials"),
                    },
                    {
                        value: 4,
                        label: this.$t("anonymization.types.fictitiousData"),
                    },
                    {
                        value: 5,
                        label: this.$t("anonymization.types.relativeReferences"),
                    },
                ],
            };
        },
        methods: {
            async fetchPrompts() {
                try {
                    this.loadingPrompts = true;
                    const userEmail = store.state.userProfile?.email || "";
                    const response = await PromptsService.getPrompts(userEmail);
                    if (response && Array.isArray(response)) {
                        this.prompts = response;
                    } else if (response && response.error) {
                        LogService.showMessage("Error fetching prompts: " + response.error);
                    }
                } catch (error) {
                    LogService.showMessage("Error fetching prompts: " + error);
                } finally {
                    this.loadingPrompts = false;
                }
            },
            open() {
                this.anonymizationType = "";
                this.promptId = "";
                this.fetchPrompts();
                this.$refs.ModalAnonymization.open();
            },
            close() {
                this.$refs.ModalAnonymization.close();
                this.$emit("close");
            },
            async confirm() {
                const params = {
                    documentId: Number(this.documentId) || parseInt(this.documentId, 10),
                    anonymizationType: this.anonymizationType
                        ? Number(this.anonymizationType)
                        : null,
                    promptId: this.promptId
                        ? Number(this.promptId) || parseInt(this.promptId, 10)
                        : null,
                };

                try {
                    this.loading = true;
                    const response = await AnonymizationServices.processAnonymization(params);
                    if (response && response.status == 200) {
                        this.$emit("success");
                        this.close();
                        this.$notify({
                            title: "anonymization.title",
                            message: "anonymization.success",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    } else {
                        this.$notify({
                            title: "anonymization.title",
                            message: "anonymization.error",
                            variant: "danger",
                            icon: "CircleXBig",
                        });
                    }
                } catch (error) {
                    this.$notify({
                        title: "anonymization.title",
                        message: "anonymization.error",
                        variant: "danger",
                        icon: "CircleXBig",
                    });
                } finally {
                    this.loading = false;
                }
            },
        },
    };
</script>

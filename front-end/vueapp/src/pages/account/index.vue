<template>
    <main class="my-account-page">
        <div class="container-fluid scroll-area my-account-page__container">
            <button
                type="button"
                class="btn btn-link my-account-page__back text-decoration-none px-0"
                @click="goBack"
            >
                <LucideIcon
                    icon="ArrowLeft"
                    :size="16"
                />
                {{ $t("common.back") }}
            </button>

            <header class="my-account-page__header">
                <h5 class="mb-1 fw-bold my-account-page__title">
                    {{ $t("account.title") }}
                </h5>
                <p class="text-muted mb-0 my-account-page__subtitle">
                    {{ $t("account.subtitle") }}
                </p>
            </header>

            <div
                v-if="isLoading"
                class="my-account-card text-muted small py-4 text-center"
            >
                {{ $t("common.loading") }}
            </div>

            <Form
                v-else
                ref="formRef"
                v-slot="{ meta }"
                autocomplete="off"
                class="my-account-card"
            >
                <section class="my-account-section">
                    <div class="my-account-section__heading">
                        <span class="my-account-section__icon">
                            <LucideIcon
                                icon="User"
                                :size="18"
                            />
                        </span>
                        <div>
                            <h6 class="mb-1 fw-semibold">
                                {{ $t("account.profileSection.title") }}
                            </h6>
                            <p class="text-muted small mb-0">
                                {{ $t("account.profileSection.description") }}
                            </p>
                        </div>
                    </div>

                    <div class="row g-3 mt-1">
                        <div class="col-12">
                            <label
                                for="accountFullName"
                                class="form-label fw-semibold mb-1"
                            >
                                {{ $t("account.profileSection.fullName") }}
                            </label>
                            <Field
                                id="accountFullName"
                                name="accountFullName"
                                type="text"
                                class="form-control"
                                :rules="'required|min:3|max:150'"
                                v-model="userData.name"
                                autocomplete="name"
                            />
                            <ErrorMessage
                                name="accountFullName"
                                class="invalid-feedback d-block"
                            />
                        </div>

                        <div class="col-12">
                            <label
                                for="accountEmail"
                                class="form-label fw-semibold mb-1 d-inline-flex align-items-center gap-1"
                            >
                                <LucideIcon
                                    icon="Mail"
                                    :size="14"
                                />
                                {{ $t("account.profileSection.email") }}
                            </label>
                            <Field
                                id="accountEmail"
                                name="accountEmail"
                                type="email"
                                class="form-control"
                                :rules="'required|min:5|max:100|email'"
                                v-model="userData.email"
                                autocomplete="email"
                            />
                            <ErrorMessage
                                name="accountEmail"
                                class="invalid-feedback d-block"
                            />
                        </div>
                    </div>
                </section>

                <hr class="my-account-card__divider" />

                <section class="my-account-section">
                    <div class="my-account-section__heading">
                        <span class="my-account-section__icon">
                            <LucideIcon
                                icon="Lock"
                                :size="18"
                            />
                        </span>
                        <div>
                            <h6 class="mb-1 fw-semibold">
                                {{ $t("account.passwordSection.title") }}
                            </h6>
                            <p class="text-muted small mb-0">
                                {{ $t("account.passwordSection.description") }}
                            </p>
                        </div>
                    </div>

                    <div class="row g-3 mt-1">
                        <div class="col-12">
                            <label
                                for="accountNewPassword"
                                class="form-label fw-semibold mb-1"
                            >
                                {{ $t("account.passwordSection.newPassword") }}
                            </label>
                            <PasswordInputComponent
                                name="accountNewPassword"
                                autocomplete="new-password"
                                :placeholder="$t('account.passwordSection.newPasswordPlaceholder')"
                                :rules="passwordRules"
                                v-model="userData.password"
                            />
                        </div>

                        <div class="col-12">
                            <label
                                for="accountConfirmPassword"
                                class="form-label fw-semibold mb-1"
                            >
                                {{ $t("account.passwordSection.confirmPassword") }}
                            </label>
                            <PasswordInputComponent
                                name="accountConfirmPassword"
                                autocomplete="new-password"
                                :placeholder="$t('account.passwordSection.confirmPasswordPlaceholder')"
                                :rules="confirmedPasswordRules"
                                v-model="userData.confirmedPassword"
                            />
                        </div>
                    </div>
                </section>

                <hr class="my-account-card__divider" />

                <footer class="my-account-card__footer d-flex align-items-center justify-content-between flex-wrap gap-2">
                    <button
                        type="button"
                        class="btn btn-outline-secondary btn-sm d-inline-flex align-items-center gap-1"
                        @click="goBack"
                    >
                        <LucideIcon
                            icon="ArrowLeft"
                            :size="14"
                        />
                        {{ $t("common.back") }}
                    </button>
                    <button
                        type="button"
                        class="btn btn-primary btn-sm d-inline-flex align-items-center gap-1"
                        :disabled="!meta.valid || isSaving"
                        @click="saveProfile"
                    >
                        <LucideIcon
                            v-if="!isSaving"
                            icon="Save"
                            :size="14"
                        />
                        <LucideIcon
                            v-else
                            icon="Loader"
                            :size="14"
                            class="animate-spin"
                        />
                        {{ $t("account.updateProfile") }}
                    </button>
                </footer>
            </Form>
        </div>
    </main>
</template>

<script>
    import { Form, Field, ErrorMessage } from "vee-validate";
    import api from "@/services/api";
    import UserService from "@/services/users/UserService";
    import PasswordInputComponent from "@/components/global/PasswordInputComponent.vue";

    export default {
        name: "MyAccountPage",
        components: {
            Form,
            Field,
            ErrorMessage,
            PasswordInputComponent,
        },
        data() {
            return {
                isLoading: true,
                isSaving: false,
                userData: {
                    id: null,
                    name: "",
                    email: "",
                    password: "",
                    confirmedPassword: "",
                    teams: [],
                },
            };
        },
        computed: {
            hasPasswordIntent() {
                return (
                    Boolean(this.userData.password?.trim()) ||
                    Boolean(this.userData.confirmedPassword?.trim())
                );
            },
            passwordRules() {
                if (!this.hasPasswordIntent) {
                    return {};
                }
                return {
                    required: true,
                    min: 6,
                    max: 50,
                    custom_password: true,
                };
            },
            confirmedPasswordRules() {
                if (!this.hasPasswordIntent) {
                    return {};
                }
                return {
                    required: true,
                    confirmed: "accountNewPassword",
                    min: 6,
                    max: 50,
                };
            },
        },
        mounted() {
            this.loadCurrentUser();
        },
        methods: {
            goBack() {
                if (window.history.length > 1) {
                    this.$router.back();
                    return;
                }
                this.$router.push({ name: "Home" });
            },
            loadCurrentUser() {
                const email = this.$store.state.userProfile.login;
                if (!email) {
                    this.isLoading = false;
                    this.goBack();
                    return;
                }

                UserService.getUserByEmail(email)
                    .then((response) => {
                        if (response?.error) {
                            this.notifyError("account.loadError");
                            this.goBack();
                            return;
                        }

                        this.userData = {
                            ...response,
                            password: "",
                            confirmedPassword: "",
                        };
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            saveProfile() {
                if (this.isSaving) return;

                const hasPassword = Boolean(this.userData.password?.trim());
                const hasConfirmation = Boolean(this.userData.confirmedPassword?.trim());

                if (hasPassword !== hasConfirmation) {
                    this.$refs.formRef?.setFieldError(
                        hasPassword ? "accountConfirmPassword" : "accountNewPassword",
                        this.$t("account.passwordSection.mismatch")
                    );
                    return;
                }

                this.isSaving = true;
                const payload = {
                    id: this.userData.id,
                    name: this.userData.name.trim(),
                    email: this.userData.email.trim(),
                    password: hasPassword ? this.userData.password : "",
                    teamIds: (this.userData.teams || []).map((team) => team.id),
                };

                api.put("User", payload)
                    .then(() => {
                        this.$store.commit("updateUserProfile", {
                            amount: {
                                ...this.$store.state.userProfile,
                                name: payload.name,
                                login: payload.email,
                            },
                        });

                        this.userData.password = "";
                        this.userData.confirmedPassword = "";
                        this.$refs.formRef?.resetForm({
                            values: {
                                accountFullName: payload.name,
                                accountEmail: payload.email,
                                accountNewPassword: "",
                                accountConfirmPassword: "",
                            },
                        });

                        this.$notify({
                            title: "account.title",
                            message: "account.saveSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    })
                    .catch(() => {
                        this.notifyError("account.saveError");
                    })
                    .finally(() => {
                        this.isSaving = false;
                    });
            },
            notifyError(messageKey) {
                this.$notify({
                    title: "account.title",
                    message: messageKey,
                    variant: "danger",
                    icon: "CircleX",
                });
            },
        },
    };
</script>

<style scoped>
    .my-account-page {
        width: 100%;
    }

    .my-account-page__container {
        max-width: 760px;
        margin: 0 auto;
        padding: 1.5rem 1.25rem 2rem;
    }

    .scroll-area {
        display: block;
        overflow-y: auto;
    }

    .my-account-page__back {
        color: var(--color-body-content, #334155);
        font-weight: 500;
        margin-bottom: 1.25rem;
    }

    .my-account-page__back:hover {
        color: var(--color-bg-btn-primary, #0d6efd);
    }

    .my-account-page__header {
        margin-bottom: 1.5rem;
    }

    .my-account-page__title {
        color: var(--color-heading-title, var(--color-body-content));
        font-size: 1.75rem;
    }

    .my-account-page__subtitle {
        font-size: 0.95rem;
    }

    .my-account-card {
        background: var(--bs-body-bg, #fff);
        border: 1px solid var(--bs-border-color, #dee2e6);
        border-radius: 0.75rem;
        padding: 1.5rem;
        box-shadow: 0 1px 3px rgba(15, 23, 42, 0.06);
    }

    .my-account-section__heading {
        display: flex;
        align-items: flex-start;
        gap: 0.75rem;
    }

    .my-account-section__icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        border-radius: 0.5rem;
        background: rgba(13, 110, 253, 0.08);
        color: var(--color-bg-btn-primary, #0d6efd);
        flex-shrink: 0;
    }

    .my-account-card__divider {
        margin: 1.5rem 0;
        opacity: 0.35;
    }

    .my-account-card :deep(.form-control) {
        border-radius: 0.5rem;
        background-color: var(--bs-tertiary-bg, #f8fafc);
        border-color: var(--bs-border-color, #dee2e6);
        min-height: 42px;
    }

    .my-account-card :deep(.form-control:focus) {
        background-color: var(--bs-body-bg, #fff);
        border-color: var(--color-bg-btn-primary, #0d6efd);
        box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.12);
    }

    .my-account-card__footer {
        padding-top: 0.25rem;
    }
</style>

<template>
    <div class="llm-models-settings">
        <p class="text-muted mb-3 llm-models-settings__subtitle">{{ $t("settings.llmModels.subtitle") }}</p>

        <div class="alert alert-primary small llm-models-settings__notice">
            <LucideIcon
                icon="Info"
                :size="16"
            />
            <span>{{ $t("settings.llmModels.persistenceNotice") }}</span>
        </div>

        <div
            v-if="loading"
            class="llm-models-settings__loading text-muted small"
        >
            {{ $t("common.loading") }}
        </div>

        <template v-else>
            <div class="llm-models-settings__list">
                <article
                    v-for="(scope, index) in scopes"
                    :key="scope.key"
                    class="llm-scope-card"
                    :class="{
                        'llm-scope-card--dirty': isCardDirty(scope),
                        'llm-scope-card--enter': !loading,
                    }"
                    :style="{ '--card-delay': `${index * 70}ms` }"
                >
                    <div class="llm-scope-card__top">
                        <div
                            class="llm-scope-card__icon"
                            :style="{ backgroundColor: scopeIconBg(scope.key) }"
                        >
                            <LucideIcon
                                :icon="scope.icon"
                                :size="20"
                                :color="scopeIconColor(scope.key)"
                            />
                        </div>

                        <div class="llm-scope-card__content">
                            <div class="llm-scope-card__header">
                                <h6 class="mb-0 fw-semibold">
                                    {{ $t(`settings.llmModels.scopes.${scope.key}.title`) }}
                                </h6>
                                <span
                                    v-if="isCardDirty(scope)"
                                    class="llm-scope-card__dirty-badge"
                                >
                                    {{ $t("settings.llmModels.unsavedChanges") }}
                                </span>
                            </div>
                            <p class="text-muted small mb-0">
                                {{ $t(`settings.llmModels.scopes.${scope.key}.description`) }}
                            </p>
                        </div>
                    </div>

                    <div class="llm-scope-card__picker">
                        <div
                            v-if="!isScopeDirty(scope.key)"
                            class="llm-model-active"
                        >
                            <span class="llm-model-active__label">
                                {{ $t("settings.llmModels.currentModel") }}
                            </span>
                            <span class="llm-model-active__name">
                                {{ modelLabel(savedModels[scope.key]) }}
                            </span>
                            <div class="llm-model-tags">
                                <span
                                    v-for="tag in findModelTags(savedModels[scope.key])"
                                    :key="`${scope.key}-active-${tag}`"
                                    class="llm-model-tag"
                                    :class="`llm-model-tag--${tagTone(tag)}`"
                                >
                                    {{ $t(`settings.llmModels.tags.${tag}`) }}
                                </span>
                            </div>
                        </div>

                        <div
                            class="llm-model-options"
                            role="radiogroup"
                            :aria-label="$t(`settings.llmModels.scopes.${scope.key}.title`)"
                        >
                            <button
                                v-for="model in models"
                                :key="`${scope.key}-${model.id}`"
                                type="button"
                                class="llm-model-option"
                                role="radio"
                                :class="{
                                    'llm-model-option--selected': localModels[scope.key] === model.id,
                                    'llm-model-option--saved':
                                        savedModels[scope.key] === model.id && !isScopeDirty(scope.key),
                                }"
                                :disabled="!canEdit || savingScope === scope.key"
                                :aria-checked="localModels[scope.key] === model.id"
                                @click="selectModel(scope.key, model.id)"
                            >
                                <span
                                    class="llm-model-option__radio"
                                    aria-hidden="true"
                                >
                                    <span class="llm-model-option__radio-dot"></span>
                                </span>
                                <span class="llm-model-option__body">
                                    <span class="llm-model-option__main">
                                        <span class="llm-model-option__name">{{ model.label }}</span>
                                        <span
                                            v-if="savedModels[scope.key] === model.id"
                                            class="llm-model-option__active-pill"
                                        >
                                            {{ $t("settings.llmModels.activeBadge") }}
                                        </span>
                                    </span>
                                    <span class="llm-model-tags">
                                        <span
                                            v-for="tag in findModelTags(model.id)"
                                            :key="`${scope.key}-${model.id}-${tag}`"
                                            class="llm-model-tag"
                                            :class="`llm-model-tag--${tagTone(tag)}`"
                                        >
                                            {{ $t(`settings.llmModels.tags.${tag}`) }}
                                        </span>
                                    </span>
                                </span>
                            </button>
                        </div>
                    </div>

                    <div
                        v-if="isScopeDirty(scope.key)"
                        class="llm-scope-card__inline-actions"
                    >
                        <button
                            type="button"
                            class="btn btn-primary btn-sm llm-scope-card__action-btn"
                            :disabled="!canEdit || savingScope === scope.key"
                            @click="saveScope(scope.key)"
                        >
                            <span
                                v-if="savingScope === scope.key"
                                class="spinner-border spinner-border-sm"
                            ></span>
                            <LucideIcon
                                v-else
                                icon="Check"
                                :size="14"
                            />
                            {{ $t("common.save") }}
                        </button>
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm llm-scope-card__action-btn"
                            :disabled="savingScope === scope.key"
                            @click="cancelScope(scope.key)"
                        >
                            <LucideIcon
                                icon="X"
                                :size="14"
                            />
                            {{ $t("common.cancel") }}
                        </button>
                    </div>

                    <details
                        v-if="scope.hasAdvancedMcp"
                        class="llm-scope-card__advanced"
                    >
                        <summary class="llm-scope-card__advanced-toggle">
                            <LucideIcon
                                icon="SlidersHorizontal"
                                :size="14"
                            />
                            <span>{{ $t("settings.llmModels.advancedSettings") }}</span>
                            <LucideIcon
                                icon="ChevronDown"
                                :size="14"
                                class="llm-scope-card__advanced-chevron"
                            />
                        </summary>

                        <div class="llm-scope-card__advanced-body">
                            <div class="llm-scope-card__mcp-header">
                                <div
                                    class="llm-scope-card__icon llm-scope-card__icon--sm"
                                    :style="{ backgroundColor: scopeIconBg('mcp') }"
                                >
                                    <LucideIcon
                                        icon="Plug"
                                        :size="16"
                                        :color="scopeIconColor('mcp')"
                                    />
                                </div>
                                <div>
                                    <div class="llm-scope-card__header">
                                        <h6 class="mb-0 fw-semibold small">
                                            {{ $t("settings.llmModels.scopes.mcp.title") }}
                                        </h6>
                                        <span class="llm-scope-card__badge">
                                            {{ $t("settings.llmModels.overrideBadge") }}
                                        </span>
                                    </div>
                                    <p class="text-muted small mb-0">
                                        {{ $t("settings.llmModels.scopes.mcp.description") }}
                                    </p>
                                </div>
                            </div>

                            <div class="llm-scope-card__callout small">
                                <LucideIcon
                                    icon="ArrowUpRight"
                                    :size="14"
                                />
                                {{ $t("settings.llmModels.mcpOverrideNotice") }}
                            </div>

                            <div
                                v-if="!isScopeDirty('mcp')"
                                class="llm-model-active llm-model-active--compact"
                            >
                                <span class="llm-model-active__label">
                                    {{ $t("settings.llmModels.currentModel") }}
                                </span>
                                <span class="llm-model-active__name">
                                    {{ modelLabel(savedModels.mcp) }}
                                </span>
                                <div class="llm-model-tags">
                                    <span
                                        v-for="tag in findModelTags(savedModels.mcp)"
                                        :key="`mcp-active-${tag}`"
                                        class="llm-model-tag"
                                        :class="`llm-model-tag--${tagTone(tag)}`"
                                    >
                                        {{ $t(`settings.llmModels.tags.${tag}`) }}
                                    </span>
                                </div>
                            </div>

                            <div
                                class="llm-model-options"
                                role="radiogroup"
                                :aria-label="$t('settings.llmModels.scopes.mcp.title')"
                            >
                                <button
                                    v-for="model in models"
                                    :key="`mcp-${model.id}`"
                                    type="button"
                                    class="llm-model-option"
                                    role="radio"
                                    :class="{
                                        'llm-model-option--selected': localModels.mcp === model.id,
                                        'llm-model-option--saved':
                                            savedModels.mcp === model.id && !isScopeDirty('mcp'),
                                    }"
                                    :disabled="!canEdit || savingScope === 'mcp'"
                                    :aria-checked="localModels.mcp === model.id"
                                    @click="selectModel('mcp', model.id)"
                                >
                                    <span
                                        class="llm-model-option__radio"
                                        aria-hidden="true"
                                    >
                                        <span class="llm-model-option__radio-dot"></span>
                                    </span>
                                    <span class="llm-model-option__body">
                                        <span class="llm-model-option__main">
                                            <span class="llm-model-option__name">{{ model.label }}</span>
                                            <span
                                                v-if="savedModels.mcp === model.id"
                                                class="llm-model-option__active-pill"
                                            >
                                                {{ $t("settings.llmModels.activeBadge") }}
                                            </span>
                                        </span>
                                        <span class="llm-model-tags">
                                            <span
                                                v-for="tag in findModelTags(model.id)"
                                                :key="`mcp-${model.id}-${tag}`"
                                                class="llm-model-tag"
                                                :class="`llm-model-tag--${tagTone(tag)}`"
                                            >
                                                {{ $t(`settings.llmModels.tags.${tag}`) }}
                                            </span>
                                        </span>
                                    </span>
                                </button>
                            </div>

                            <div
                                v-if="isScopeDirty('mcp')"
                                class="llm-scope-card__inline-actions"
                            >
                                <button
                                    type="button"
                                    class="btn btn-primary btn-sm llm-scope-card__action-btn"
                                    :disabled="!canEdit || savingScope === 'mcp'"
                                    @click="saveScope('mcp')"
                                >
                                    <span
                                        v-if="savingScope === 'mcp'"
                                        class="spinner-border spinner-border-sm"
                                    ></span>
                                    <LucideIcon
                                        v-else
                                        icon="Check"
                                        :size="14"
                                    />
                                    {{ $t("common.save") }}
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-outline-secondary btn-sm llm-scope-card__action-btn"
                                    :disabled="savingScope === 'mcp'"
                                    @click="cancelScope('mcp')"
                                >
                                    <LucideIcon
                                        icon="X"
                                        :size="14"
                                    />
                                    {{ $t("common.cancel") }}
                                </button>
                            </div>
                        </div>
                    </details>
                </article>
            </div>

            <p
                v-if="!canEdit"
                class="text-muted small mb-0 llm-models-settings__readonly"
            >
                {{ $t("settings.llmModels.readOnlyNotice") }}
            </p>
        </template>
    </div>
</template>

<script>
    import {
        findModelTags,
        MODEL_TAG_TONES,
        PLATFORM_LLM_SCOPES,
    } from "@/services/settings/llmModelsConstants";
    import {
        loadLlmModelsSettingsFromApi,
        saveLlmModelsSettingsToApi,
    } from "@/services/settings/llmModelsSettings";

    const SCOPE_COLORS = {
        agents: { bg: "var(--chip-prompt-bg)", color: "var(--chip-prompt-text)" },
        questionnaires: { bg: "var(--chip-quiz-bg)", color: "var(--chip-quiz-text)" },
        documents: { bg: "var(--chip-api-bg)", color: "var(--chip-api-text)" },
        mcp: { bg: "var(--chip-n8n-bg)", color: "var(--chip-n8n-text)" },
    };

    export default {
        name: "LlmModelsSettings",
        data() {
            return {
                models: [],
                scopes: PLATFORM_LLM_SCOPES,
                localModels: {},
                savedModels: {},
                canEdit: false,
                loading: true,
                savingScope: null,
            };
        },
        mounted() {
            this.load();
        },
        methods: {
            findModelTags,
            scopeIconBg(key) {
                return SCOPE_COLORS[key]?.bg || "var(--color-bg-body-content)";
            },
            scopeIconColor(key) {
                return SCOPE_COLORS[key]?.color || "var(--color-body-content)";
            },
            tagTone(tag) {
                return MODEL_TAG_TONES[tag] || "neutral";
            },
            modelLabel(modelId) {
                return this.models.find((model) => model.id === modelId)?.label || modelId;
            },
            isScopeDirty(scopeKey) {
                return this.localModels[scopeKey] !== this.savedModels[scopeKey];
            },
            isCardDirty(scope) {
                if (this.isScopeDirty(scope.key)) return true;
                return scope.hasAdvancedMcp && this.isScopeDirty("mcp");
            },
            selectModel(scopeKey, modelId) {
                if (!this.canEdit || this.savingScope) return;
                this.localModels = { ...this.localModels, [scopeKey]: modelId };
            },
            cancelScope(scopeKey) {
                this.localModels = { ...this.localModels, [scopeKey]: this.savedModels[scopeKey] };
            },
            load() {
                this.loading = true;
                return loadLlmModelsSettingsFromApi()
                    .then((settings) => {
                        this.models = settings.availableModels;
                        this.localModels = { ...settings.models };
                        this.savedModels = { ...settings.models };
                        this.canEdit = settings.canEdit;
                    })
                    .finally(() => {
                        this.loading = false;
                    });
            },
            saveScope(scopeKey) {
                if (!this.canEdit || !this.isScopeDirty(scopeKey)) return;

                this.savingScope = scopeKey;
                saveLlmModelsSettingsToApi({ models: { ...this.localModels } })
                    .then((settings) => {
                        this.models = settings.availableModels;
                        this.savedModels = { ...settings.models };
                        this.localModels = { ...settings.models };
                        this.$notify({
                            title: "settings.llmModels.title",
                            message: "settings.llmModels.saved",
                            variant: "success",
                            icon: "check",
                        });
                    })
                    .finally(() => {
                        this.savingScope = null;
                    });
            },
        },
    };
</script>

<style scoped>
    .llm-models-settings__subtitle {
        max-width: 52rem;
    }

    .llm-models-settings__notice {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
        margin-bottom: 1.25rem;
    }

    .llm-models-settings__list {
        display: grid;
        grid-template-columns: 1fr;
        gap: 1rem;
    }

    .llm-models-settings__readonly {
        margin-top: 1rem;
        padding-top: 0.75rem;
        border-top: 1px solid var(--color-border-form-control);
    }

    .llm-scope-card {
        padding: 1.1rem 1.15rem;
        border: 1px solid var(--color-border-form-control);
        border-radius: 12px;
        background: var(--color-bg-body-content);
        transition: border-color 0.2s ease, box-shadow 0.2s ease;
    }

    .llm-scope-card--enter {
        animation: llm-card-enter 0.45s ease both;
        animation-delay: var(--card-delay, 0ms);
    }

    .llm-scope-card--dirty {
        border-color: color-mix(in srgb, var(--color-btn-outline-primary) 45%, var(--color-border-form-control));
        box-shadow: 0 0 0 1px color-mix(in srgb, var(--color-btn-outline-primary) 12%, transparent);
    }

    .llm-scope-card__top {
        display: grid;
        grid-template-columns: 44px minmax(0, 1fr);
        gap: 0.85rem;
        margin-bottom: 1rem;
    }

    .llm-scope-card__icon {
        flex-shrink: 0;
        width: 44px;
        height: 44px;
        border-radius: 10px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }

    .llm-scope-card__icon--sm {
        width: 36px;
        height: 36px;
        border-radius: 8px;
    }

    .llm-scope-card__header {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 0.5rem;
        margin-bottom: 0.35rem;
    }

    .llm-scope-card__header h6 {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .llm-scope-card__dirty-badge {
        font-size: 0.68rem;
        font-weight: 600;
        letter-spacing: 0.02em;
        padding: 0.15rem 0.45rem;
        border-radius: 999px;
        background: color-mix(in srgb, var(--color-btn-outline-primary) 14%, transparent);
        color: var(--color-btn-outline-primary);
    }

    .llm-scope-card__badge {
        font-size: 0.68rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.03em;
        padding: 0.15rem 0.45rem;
        border-radius: 999px;
        background: rgba(255, 224, 130, 0.2);
        color: rgba(236, 109, 24, 1);
    }

    .llm-scope-card__callout {
        display: flex;
        align-items: flex-start;
        gap: 0.35rem;
        margin-bottom: 0.85rem;
        padding: 0.55rem 0.7rem;
        border-radius: 8px;
        background: var(--color-bg-primary-badge);
        color: var(--color-text-primary-badge);
    }

    .llm-model-active {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.45rem 0.65rem;
        margin-bottom: 0.75rem;
        padding: 0.55rem 0.75rem;
        border-radius: 8px;
        background: color-mix(in srgb, var(--color-card-content) 70%, var(--color-bg-body-content));
        border: 1px solid var(--color-border-form-control);
    }

    .llm-model-active--compact {
        margin-top: 0.25rem;
    }

    .llm-model-active__label {
        font-size: 0.72rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--color-text-muted);
    }

    .llm-model-active__name {
        font-size: 0.88rem;
        font-weight: 600;
        color: var(--color-heading-title, var(--color-body-content));
    }

    .llm-model-options {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
        gap: 0.55rem;
    }

    .llm-model-option {
        display: flex;
        flex-direction: row;
        align-items: flex-start;
        gap: 0.65rem;
        width: 100%;
        padding: 0.65rem 0.75rem;
        border: 1px solid var(--color-border-form-control);
        border-radius: 10px;
        background: var(--color-card-content);
        text-align: left;
        cursor: pointer;
        transition: border-color 0.15s ease, background 0.15s ease, transform 0.15s ease;
    }

    .llm-model-option__radio {
        flex-shrink: 0;
        width: 18px;
        height: 18px;
        margin-top: 0.1rem;
        border: 2px solid var(--color-border-form-control);
        border-radius: 50%;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: var(--color-bg-body-content);
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

    .llm-model-option__radio-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: var(--color-btn-outline-primary);
        transform: scale(0);
        opacity: 0;
        transition: transform 0.15s ease, opacity 0.15s ease;
    }

    .llm-model-option__body {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.45rem;
        min-width: 0;
        flex: 1;
    }

    .llm-model-option:hover:not(:disabled) {
        border-color: color-mix(in srgb, var(--color-btn-outline-primary) 35%, var(--color-border-form-control));
        transform: translateY(-1px);
    }

    .llm-model-option:disabled {
        opacity: 0.65;
        cursor: not-allowed;
    }

    .llm-model-option--selected {
        border-color: var(--color-btn-outline-primary);
        background: color-mix(in srgb, var(--color-btn-outline-primary) 8%, var(--color-card-content));
        box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--color-btn-outline-primary) 25%, transparent);
    }

    .llm-model-option--selected .llm-model-option__radio {
        border-color: var(--color-btn-outline-primary);
        box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-btn-outline-primary) 12%, transparent);
    }

    .llm-model-option--selected .llm-model-option__radio-dot {
        transform: scale(1);
        opacity: 1;
    }

    .llm-model-option:hover:not(:disabled) .llm-model-option__radio {
        border-color: color-mix(in srgb, var(--color-btn-outline-primary) 50%, var(--color-border-form-control));
    }

    .llm-model-option--saved:not(.llm-model-option--selected) {
        border-style: dashed;
    }

    .llm-model-option__main {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 0.4rem;
        width: 100%;
    }

    .llm-model-option__name {
        font-size: 0.84rem;
        font-weight: 600;
        color: var(--color-heading-title, var(--color-body-content));
    }

    .llm-model-option__active-pill {
        font-size: 0.62rem;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        padding: 0.12rem 0.4rem;
        border-radius: 999px;
        background: color-mix(in srgb, var(--color-btn-outline-primary) 16%, transparent);
        color: var(--color-btn-outline-primary);
    }

    .llm-model-tags {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
    }

    .llm-model-tag {
        font-size: 0.64rem;
        font-weight: 600;
        letter-spacing: 0.02em;
        padding: 0.12rem 0.38rem;
        border-radius: 999px;
        line-height: 1.2;
    }

    .llm-model-tag--neutral {
        background: var(--color-bg-body-content);
        color: var(--color-text-muted);
        border: 1px solid var(--color-border-form-control);
    }

    .llm-model-tag--primary {
        background: var(--color-bg-primary-badge);
        color: var(--color-text-primary-badge);
    }

    .llm-model-tag--success {
        background: color-mix(in srgb, #22c55e 14%, transparent);
        color: #15803d;
    }

    .llm-model-tag--warning {
        background: rgba(255, 224, 130, 0.2);
        color: rgba(236, 109, 24, 1);
    }

    .llm-model-tag--info {
        background: color-mix(in srgb, #0ea5e9 14%, transparent);
        color: #0369a1;
    }

    .llm-model-tag--accent {
        background: var(--chip-n8n-bg);
        color: var(--chip-n8n-text);
    }

    .llm-scope-card__inline-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 0.45rem;
        margin-top: 0.85rem;
        padding-top: 0.85rem;
        border-top: 1px dashed var(--color-border-form-control);
    }

    .llm-scope-card__action-btn {
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
    }

    .llm-scope-card__advanced {
        margin-top: 1rem;
        border-top: 1px solid var(--color-border-form-control);
        padding-top: 0.75rem;
    }

    .llm-scope-card__advanced-toggle {
        display: flex;
        align-items: center;
        gap: 0.45rem;
        list-style: none;
        cursor: pointer;
        font-size: 0.82rem;
        font-weight: 600;
        color: var(--color-text-muted);
        user-select: none;
        padding: 0.35rem 0;
        transition: color 0.15s ease;
    }

    .llm-scope-card__advanced-toggle::-webkit-details-marker {
        display: none;
    }

    .llm-scope-card__advanced-toggle:hover {
        color: var(--color-body-content);
    }

    .llm-scope-card__advanced-chevron {
        margin-left: auto;
        transition: transform 0.2s ease;
    }

    .llm-scope-card__advanced[open] .llm-scope-card__advanced-chevron {
        transform: rotate(180deg);
    }

    .llm-scope-card__advanced-body {
        margin-top: 0.85rem;
        padding: 0.85rem;
        border-radius: 10px;
        border: 1px dashed var(--color-border-form-control);
        background: color-mix(in srgb, var(--color-card-content) 55%, var(--color-bg-body-content));
    }

    .llm-scope-card__mcp-header {
        display: grid;
        grid-template-columns: 36px minmax(0, 1fr);
        gap: 0.65rem;
        margin-bottom: 0.75rem;
    }

    @keyframes llm-card-enter {
        from {
            opacity: 0;
            transform: translateY(8px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @media (max-width: 576px) {
        .llm-scope-card__top {
            grid-template-columns: 1fr;
        }

        .llm-model-options {
            grid-template-columns: 1fr;
        }
    }
</style>

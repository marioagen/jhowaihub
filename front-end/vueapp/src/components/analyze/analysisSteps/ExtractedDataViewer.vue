<template>
    <div class="extracted-fields-container">
        <div>
            <h6 class="section-title">
                <i class="fas fa-database"></i>
                {{ title }}
            </h6>

            <div
                v-if="fields.length === 0"
                class="no-data-message"
            >
                <i class="fas fa-info-circle"></i>
                {{ $t("analyze.noDataInDocument") }}
            </div>

            <div
                v-else
                class="fields-list"
            >
                <div
                    v-for="(field, index) in fields"
                    :key="index"
                    class="field-item"
                >
                    <div
                        v-if="field.outputType?.toLowerCase() !== 'api'"
                        class="field-header"
                    >
                        <div class="field-header-main">
                            <label
                                v-if="field.outputType !== 'Prompt' && field.outputType !== 'Quiz'"
                                class="field-label"
                            >
                                {{ field.label }}
                            </label>
                            <div class="field-meta">
                                <span
                                    :class="['tool-chip', toolTypeClass(field)]"
                                    :title="mergedChipLabel(field)"
                                >
                                    {{ mergedChipLabel(field) }}
                                </span>
                            </div>
                        </div>
                        <div class="field-header-actions">
                            <span
                                v-if="field.isEdited"
                                class="edited-badge"
                            >
                                <i class="fas fa-pen"></i>
                                {{ $t("common.edited") }}
                            </span>
                            <span
                                v-if="field.outputType !== 'Quiz'"
                                class="field-header-eye"
                                @click="open(fields[index].value, field.label, index)"
                            >
                                <LucideIcon
                                    icon="Eye"
                                    :size="16"
                                />
                            </span>
                        </div>
                    </div>
                    <div
                        class="field-value-container"
                        v-if="field.outputType == 'N8N'"
                    >
                        <input
                            type="text"
                            class="field-value"
                            @input="(e) => handleFieldEdit(index, e.target.value)"
                            :readonly="!isEditing[index]"
                            v-model="fields[index].value"
                        />
                        <button
                            v-if="!isEditing[index]"
                            class="edit-button mb-2"
                            @click="startEditing(index)"
                            :title="$t('common.edit')"
                        >
                            <i class="fas fa-pen"></i>
                        </button>
                        <div
                            v-else
                            class="edit-actions"
                        >
                            <button
                                class="save-button"
                                @click="saveEdit(index, field.outputId)"
                                :title="$t('common.save')"
                            >
                                <i class="fas fa-check"></i>
                            </button>
                            <button
                                class="cancel-button"
                                @click="cancelEdit(index)"
                                :title="$t('common.cancel')"
                            >
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                    <div v-if="field.outputType == 'Prompt'">
                        <textarea
                            type="text"
                            class="form-control mb-2"
                            @input="(e) => handleFieldEdit(index, e.target.value)"
                            :readonly="!isEditing[index]"
                            v-model="fields[index].value"
                            rows="5"
                        ></textarea>
                        <button
                            v-if="!isEditing[index]"
                            class="edit-button mb-2"
                            @click="startEditing(index)"
                            :title="$t('common.edit')"
                        >
                            <i class="fas fa-pen"></i>
                        </button>
                        <div
                            v-else
                            class="edit-actions"
                        >
                            <button
                                class="save-button"
                                @click="saveEdit(index, field.outputId)"
                                :title="$t('common.save')"
                            >
                                <i class="fas fa-check"></i>
                            </button>
                            <button
                                class="cancel-button"
                                @click="cancelEdit(index)"
                                :title="$t('common.cancel')"
                            >
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                    <div v-if="field.outputType?.toLowerCase() === 'api'">
                        <div
                            v-if="field.label == 'TemplateName'"
                            :class="
                                index == 0
                                    ? 'field-header field-header-api-title'
                                    : 'field-header field-header-api-title border-top pt-4'
                            "
                        >
                            <div class="field-meta field-meta-api mb-2">
                                <span
                                    :class="['tool-chip', toolTypeClass(field)]"
                                    :title="mergedApiChipLabel(field)"
                                >
                                    {{ mergedApiChipLabel(field) }}
                                </span>
                            </div>
                        </div>
                        <div v-else>
                            <label class="field-label">
                                {{ field.label }}
                            </label>
                            <input
                                v-if="field.label === 'StatusCode'"
                                type="text"
                                class="field-value form-control mt-2"
                                readonly
                                v-model="fields[index].value"
                            />
                            <textarea
                                v-else
                                type="text"
                                class="form-control mt-2"
                                readonly
                                v-model="fields[index].value"
                                rows="9"
                            ></textarea>
                        </div>
                    </div>
                    <div v-if="field.outputType == 'Quiz'">
                        <div
                            v-for="item in getTheValues(field)"
                            :key="item.outputId"
                            class="block"
                        >
                            <div class="question">
                                <label class="field-label">{{ item.Question }}</label>
                                <p>{{ item.Answer }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <ExtractDataModal
        ref="ExtractModal"
        @update="handleModalUpdate"
    />
</template>
<script>
    import ExtractDataModal from "@/components/analyze/ExtractDataModal.vue";
    export default {
        name: "ExtractedFields",
        props: {
            fields: {
                type: Array,
                required: true,
                default: () => [],
            },
            title: {
                type: String,
                default: "Dados Extraídos",
            },
            value: {
                type: String,
                default: "",
            },
            label: {
                type: String,
                default: "",
            },
        },
        emits: ["field-updated"],
        components: {
            ExtractDataModal,
        },
        data() {
            return {
                isEditing: {},
                originalValues: {},
                currentModalIndex: null,
            };
        },
        methods: {
            outputTypeLabel(field) {
                if (!field || !field.outputType) return "";
                const key = `connectors.typeDisplay.${field.outputType}`;
                const translated = this.$t(key);
                return translated && translated !== key ? translated : field.outputType;
            },
            toolNameChip(field) {
                const n = field?.toolName && String(field.toolName).trim();
                return n || "";
            },
            toolTypeClass(field) {
                const map = {
                    prompt: "chip-prompt",
                    quiz: "chip-quiz",
                    api: "chip-api",
                    n8n: "chip-n8n",
                    embeddings: "chip-embeddings",
                };
                return map[field.outputType?.toLowerCase()] || "chip-default";
            },
            mergedChipLabel(field) {
                const type = this.outputTypeLabel(field);
                const tool = this.toolNameChip(field);
                if (type && tool) return `${type} - ${tool}`;
                return type || tool || "";
            },
            mergedApiChipLabel(field) {
                const type = this.outputTypeLabel(field);
                const templateName = (field.value || "").trim();
                if (templateName && templateName.toLowerCase() !== type.toLowerCase()) {
                    return `${type} - ${templateName}`;
                }
                return type;
            },
            startEditing(index) {
                this.originalValues[index] = this.fields[index].value;
                this.isEditing[index] = true;
            },
            handleFieldEdit(index, value) {
                const updatedField = {
                    ...this.fields[index],
                    value,
                };
                this.$emit("field-changed", {
                    index,
                    field: updatedField,
                });
            },
            saveEdit(index, id) {
                this.isEditing[index] = false;
                this.fields[index].isEdited = true;
                if (this.fields[index].outputType === "N8N") {
                    const outputsObj = {};
                    this.fields.forEach((field) => {
                        outputsObj[field.label] = field.value;
                    });
                    const outputsJson = JSON.stringify(outputsObj);

                    this.$emit("field-updated", {
                        id,
                        field: this.fields[index],
                        outputsJson,
                    });
                    return;
                }
                this.$emit("field-updated", {
                    id,
                    field: this.fields[index],
                });
            },
            cancelEdit(index) {
                const restoredField = {
                    ...this.fields[index],
                    value: this.originalValues[index],
                };
                this.$emit("field-changed", {
                    index,
                    field: restoredField,
                });
                this.isEditing[index] = false;
                delete this.originalValues[index];
            },
            open(value, label, index) {
                this.currentModalIndex = index;
                this.$refs.ExtractModal.open(value, label);
            },
            handleModalUpdate(newValue) {
                if (this.currentModalIndex === null) return;

                const index = this.currentModalIndex;
                const field = this.fields[index];

                this.fields[index].value = newValue;
                this.fields[index].isEdited = true;

                if (field.outputType === "N8N") {
                    const outputsObj = {};
                    this.fields.forEach((field) => {
                        outputsObj[field.label] = field.value;
                    });
                    const outputsJson = JSON.stringify(outputsObj);

                    this.$emit("field-updated", {
                        id: field.outputId,
                        field: this.fields[index],
                        outputsJson,
                    });
                } else {
                    this.$emit("field-updated", {
                        id: field.outputId,
                        field: this.fields[index],
                    });
                }
                this.currentModalIndex = null;
            },
            getTheValues(item) {
                let parsedValue = [];

                try {
                    parsedValue = JSON.parse(item.value);
                } catch (e) {
                    parsedValue = [];
                }

                return parsedValue;
            },
        },
    };
</script>
<style scoped>
    .extracted-fields-container {
        background: var(--color-card-content) !important;
        border-radius: 8px;
        padding: 1rem;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        height: 100%;
        max-height: calc(100vh - 400px);
        overflow-y: auto;
    }

    .section-title {
        color: var(--color-body-content);
        font-size: 1rem;
        font-weight: 600;
        margin-bottom: 1rem;
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .no-data-message {
        text-align: center;
        color: var(--color-text-body-content);
        padding: 2rem;
        font-size: 0.95rem;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
    }

    .fields-list {
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }

    .field-item {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .field-header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 0.75rem;
    }

    .field-header-main {
        flex: 1;
        min-width: 0;
    }

    .field-header-actions {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        flex-shrink: 0;
    }

    .field-header-eye {
        cursor: pointer;
        display: flex;
        align-items: center;
    }

    .field-meta {
        display: flex;
        flex-wrap: wrap;
        gap: 0.35rem;
        margin-top: 0.35rem;
        align-items: center;
    }

    .field-meta-api {
        margin-top: 0;
    }

    .field-header-api-title {
        flex-direction: column;
        align-items: stretch;
    }

    .tool-chip {
        display: inline-flex;
        align-items: center;
        border-radius: 12px;
        padding: 0.12rem 0.65rem;
        font-size: 0.72rem;
        font-weight: 600;
        line-height: 1.4;
        border-width: 1px;
        border-style: solid;
        max-width: min(100%, 320px);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .chip-prompt {
        background-color: var(--chip-prompt-bg);
        color: var(--chip-prompt-text);
        border-color: var(--chip-prompt-border);
    }

    .chip-quiz {
        background-color: var(--chip-quiz-bg);
        color: var(--chip-quiz-text);
        border-color: var(--chip-quiz-border);
    }

    .chip-api {
        background-color: var(--chip-api-bg);
        color: var(--chip-api-text);
        border-color: var(--chip-api-border);
    }

    .chip-n8n {
        background-color: var(--chip-n8n-bg);
        color: var(--chip-n8n-text);
        border-color: var(--chip-n8n-border);
    }

    .chip-embeddings {
        background-color: var(--chip-embeddings-bg);
        color: var(--chip-embeddings-text);
        border-color: var(--chip-embeddings-border);
    }

    .chip-default {
        background-color: var(--chip-default-bg);
        color: var(--chip-default-text);
        border-color: var(--chip-default-border);
    }

    .field-label {
        font-size: 0.85rem;
        color: var(--color-body-content);
        font-weight: 500;
        margin: 0;
    }

    .edited-badge {
        background: #ff9800;
        color: white;
        padding: 0.2rem 0.6rem;
        border-radius: 12px;
        font-size: 0.75rem;
        display: flex;
        align-items: center;
        gap: 0.3rem;
    }

    .field-value-container {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .field-value {
        flex: 1;
        padding: 0.6rem;
        border: 1px solid var(--color-border-form-control);
        border-radius: 4px;
        font-size: 0.9rem;
        transition: border-color 0.3s ease;
    }

    .field-value:focus {
        outline: none;
        border-color: #0073e6;
    }

    .field-value:read-only {
        background-color: var(--color-read-only) !important;
    }

    .edit-button,
    .save-button,
    .cancel-button {
        width: 32px;
        height: 32px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
    }

    .edit-button {
        background: #0073e6;
        color: white;
    }

    .edit-button:hover {
        background: #005bb5;
    }

    .edit-actions {
        display: flex;
        gap: 0.5rem;
    }

    .save-button {
        background: #28a745;
        color: white;
    }

    .save-button:hover {
        background: #218838;
    }

    .cancel-button {
        background: #6c757d;
        color: white;
    }

    .cancel-button:hover {
        background: #5a6268;
    }

    @media (max-width: 768px) {
        .extracted-fields-container {
            border-radius: 8px;
            padding: 1rem;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .section-title {
            font-size: 1rem;
            font-weight: 600;
            margin-bottom: 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .no-data-message {
            text-align: center;
            padding: 2rem;
            font-size: 0.95rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
        }

        .fields-list {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .field-item {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .field-header {
            display: flex;
            align-items: flex-start;
            justify-content: space-between;
            gap: 0.75rem;
        }

        .field-label {
            font-size: 0.85rem;
        }

        .edited-badge {
            background: #ff9800;
            color: white;
            padding: 0.2rem 0.6rem;
            border-radius: 12px;
            font-size: 0.75rem;
            display: flex;
            align-items: center;
            gap: 0.3rem;
        }

        .field-value-container {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .field-value {
            flex: 1;
            padding: 0.6rem;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 0.9rem;
            transition: border-color 0.3s ease;
        }

        .field-value:focus {
            outline: none;
            border-color: #0073e6;
        }

        .field-value:read-only {
            background-color: #f8f9fa;
        }

        .edit-button,
        .save-button,
        .cancel-button {
            width: 32px;
            height: 32px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
        }

        .edit-button {
            background: #0073e6;
            color: white;
        }

        .edit-button:hover {
            background: #005bb5;
        }

        .edit-actions {
            display: flex;
            gap: 0.5rem;
        }

        .save-button {
            background: #28a745;
            color: white;
        }

        .save-button:hover {
            background: #218838;
        }

        .cancel-button {
            background: #6c757d;
            color: white;
        }

        .cancel-button:hover {
            background: #5a6268;
        }

        @media (max-width: 768px) {
            .extracted-fields-container {
                padding: 0.75rem;
            }

            .field-label {
                font-size: 0.8rem;
            }

            .field-value {
                padding: 0.5rem;
                font-size: 0.85rem;
            }
        }
    }

    .block {
        margin-bottom: 20px;
    }

    .question {
        padding: 10px;
        border: 1px solid var(--color-border-form-control);
        margin-top: 10px;
    }

    .border-top {
        border-top: 1px solid var(--color-border-form-control) !important;
    }
</style>

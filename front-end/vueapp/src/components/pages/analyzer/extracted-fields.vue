<template>
    <div class="extracted-fields-container">
        <h6 class="section-title">
            <i class="fas fa-database"></i>
            {{ title }}
        </h6>

        <div v-if="fields.length === 0" class="no-data-message">
            <i class="fas fa-info-circle"></i>
            {{ $t("labelNoDataInDocument") }}
        </div>

        <div v-else class="fields-list">
            <div v-for="(field, index) in fields" :key="index" class="field-item">
                <div class="field-header">
                    <label class="field-label">{{ field.label }}</label>
                    <span v-if="field.isEdited" class="edited-badge">
                        <i class="fas fa-pen"></i>
                        {{ $t("labelEdited") }}
                    </span>
                </div>
                <div class="field-value-container">
                    <input
                        type="text"
                        class="field-value"
                        :value="field.value"
                        @input="(e) => handleFieldEdit(index, e.target.value)"
                        :readonly="!isEditing[index]"
                    />
                    <button
                        v-if="!isEditing[index]"
                        class="edit-button"
                        @click="startEditing(index)"
                        :title="$t('labelEdit')"
                    >
                        <i class="fas fa-pen"></i>
                    </button>
                    <div v-else class="edit-actions">
                        <button class="save-button" @click="saveEdit(index)" :title="$t('labelSave')">
                            <i class="fas fa-check"></i>
                        </button>
                        <button class="cancel-button" @click="cancelEdit(index)" :title="$t('labelCancel')">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
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
        },
        emits: ["field-updated"],
        data() {
            return {
                isEditing: {},
                originalValues: {},
            };
        },
        methods: {
            startEditing(index) {
                this.originalValues[index] = this.fields[index].value;
                this.isEditing[index] = true;
            },
            handleFieldEdit(index, value) {
                // Emit the change instead of directly mutating the prop
                const updatedField = { ...this.fields[index], value };
                this.$emit("field-changed", { index, field: updatedField });
            },
            saveEdit(index) {
                const field = { ...this.fields[index], isEdited: true };
                this.isEditing[index] = false;
                this.$emit("field-updated", { index, field });
            },
            cancelEdit(index) {
                const restoredField = { ...this.fields[index], value: this.originalValues[index] };
                this.$emit("field-changed", { index, field: restoredField });
                this.isEditing[index] = false;
                delete this.originalValues[index];
            },
        },
    };
</script>

<style scoped>
    .extracted-fields-container {
        background: white;
        border-radius: 8px;
        padding: 1rem;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .section-title {
        color: #0073e6;
        font-size: 1rem;
        font-weight: 600;
        margin-bottom: 1rem;
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .no-data-message {
        text-align: center;
        color: #666;
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
        align-items: center;
        justify-content: space-between;
    }

    .field-label {
        font-size: 0.85rem;
        color: #333;
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
</style>

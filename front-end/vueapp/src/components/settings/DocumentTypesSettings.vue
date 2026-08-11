<template>
    <section class="document-types-settings">
        <header class="document-types-settings__header">
            <div><h6 class="fw-bold mb-1">{{ $t("contextDossiers.typesSettings.title") }}</h6><p class="text-muted small mb-0">{{ $t("contextDossiers.typesSettings.subtitle") }}</p></div>
            <button class="btn btn-primary btn-sm" @click="openForm()"><LucideIcon icon="Plus" :size="15" /> {{ $t("contextDossiers.typesSettings.add") }}</button>
        </header>
        <div class="alert alert-primary small"><LucideIcon icon="FlaskConical" :size="16" /> {{ $t("contextDossiers.typesSettings.notice") }}</div>
        <div class="document-types-settings__table-wrap"><table class="table mb-0">
            <thead><tr><th>{{ $t("contextDossiers.typesSettings.name") }}</th><th>{{ $t("contextDossiers.typesSettings.group") }}</th><th>{{ $t("contextDossiers.typesSettings.status") }}</th><th></th></tr></thead>
            <tbody><tr v-for="type in types" :key="type.id">
                <td class="fw-semibold">{{ type.name }}</td><td>{{ $t(`contextDossiers.typeGroups.${type.group}`) }}</td>
                <td><button class="btn btn-sm" :class="type.active ? 'btn-outline-success' : 'btn-outline-secondary'" @click="toggle(type)">{{ type.active ? $t("contextDossiers.typesSettings.active") : $t("contextDossiers.typesSettings.inactive") }}</button></td>
                <td class="text-end"><button class="btn btn-link btn-sm" :title="$t('common.edit')" @click="openForm(type)"><LucideIcon icon="Pencil" :size="16" /></button><button class="btn btn-link btn-sm text-danger" :title="$t('common.delete')" @click="remove(type)"><LucideIcon icon="Trash2" :size="16" /></button></td>
            </tr></tbody>
        </table></div>
        <ModalComponent ref="modal" id="document-type-form" :title="form.id ? 'contextDossiers.typesSettings.editTitle' : 'contextDossiers.typesSettings.createTitle'" @save="save">
            <div class="mb-3"><label for="type-name" class="form-label">{{ $t("contextDossiers.typesSettings.name") }}</label><input id="type-name" v-model="form.name" class="form-control" /></div>
            <div><label for="type-group" class="form-label">{{ $t("contextDossiers.typesSettings.group") }}</label><select id="type-group" v-model="form.group" class="form-select"><option value="legal">{{ $t("contextDossiers.typeGroups.legal") }}</option><option value="financial">{{ $t("contextDossiers.typeGroups.financial") }}</option><option value="other">{{ $t("contextDossiers.typeGroups.other") }}</option></select></div>
        </ModalComponent>
    </section>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import { deleteDocumentType, loadDocumentTypes, saveDocumentType } from "@/services/documents/contextDossierStorage";
    const EMPTY_FORM = { id: null, name: "", group: "legal", active: true };
    export default {
        name: "DocumentTypesSettings", components: { ModalComponent },
        data() { return { types: [], form: { ...EMPTY_FORM } }; }, mounted() { this.reload(); },
        methods: {
            reload() { this.types = loadDocumentTypes(); },
            openForm(type = null) { this.form = type ? { ...type } : { ...EMPTY_FORM }; this.$refs.modal.open(); },
            save() { if (!this.form.name.trim()) return; saveDocumentType({ ...this.form, name: this.form.name.trim() }); this.$refs.modal.close(); this.reload(); },
            toggle(type) { saveDocumentType({ ...type, active: !type.active }); this.reload(); },
            remove(type) { if (window.confirm(this.$t("contextDossiers.typesSettings.deleteConfirm"))) { deleteDocumentType(type.id); this.reload(); } },
        },
    };
</script>
<style scoped>
    .document-types-settings__header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
    .document-types-settings__table-wrap { overflow-x: auto; border: 1px solid var(--color-border-form-control); border-radius: 8px; }
    table { min-width: 600px; } th { background: var(--color-bg-body-content); color: var(--color-text-muted); font-size: .76rem; } td { background: var(--color-card-content); color: var(--color-body-content); vertical-align: middle; }
    @media (max-width: 576px) { .document-types-settings__header { flex-direction: column; } .document-types-settings__header .btn { min-height: 44px; width: 100%; } }
</style>

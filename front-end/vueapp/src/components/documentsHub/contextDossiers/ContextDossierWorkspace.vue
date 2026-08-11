<template>
    <main
        v-if="dossier"
        class="dossier-workspace"
        tabindex="0"
        :aria-label="$t('contextDossiers.title')"
    >
        <header class="dossier-workspace__header">
            <div class="d-flex align-items-start gap-2">
                <button type="button" class="btn btn-outline-secondary btn-sm dossier-workspace__back" :title="$t('common.back')" @click="back">
                    <LucideIcon icon="ArrowLeft" :size="17" />
                </button>
                <div>
                    <div class="d-flex align-items-center gap-2 flex-wrap">
                        <input v-model="dossier.name" class="dossier-workspace__name" :aria-label="$t('contextDossiers.form.name')" @change="persist" />
                        <span class="dossiers-status" :class="`dossiers-status--${dossierStatus}`">{{ $t(`contextDossiers.status.${dossierStatus}`) }}</span>
                    </div>
                    <p class="text-muted small mb-0">{{ $t("contextDossiers.prototypeNotice") }}</p>
                </div>
            </div>
            <div class="dossier-workspace__primary-actions">
                <button type="button" class="btn btn-outline-primary btn-sm" :disabled="!canPrepare" @click="prepare">
                    <LucideIcon icon="WandSparkles" :size="15" /> {{ $t("contextDossiers.prepare") }}
                </button>
                <button type="button" class="btn btn-primary btn-sm" :disabled="dossierStatus !== 'prepared'" @click="openDispatch">
                    <LucideIcon icon="Send" :size="15" /> {{ $t("contextDossiers.send") }}
                </button>
            </div>
        </header>

        <div
            class="dossier-workspace__layout"
            :class="{ 'dossier-workspace__layout--focus': filesPanelCollapsed }"
        >
            <aside
                class="dossier-workspace__files"
                :class="{
                    'd-none d-lg-flex': mobileDetail,
                    'd-lg-none': filesPanelCollapsed,
                }"
            >
                <div class="dossier-workspace__files-header">
                    <div>
                        <h6 class="mb-0">{{ $t("contextDossiers.filesTitle") }}</h6>
                        <small class="text-muted">{{ $t("contextDossiers.filesCount", { count: dossier.files.length }) }}</small>
                    </div>
                    <div class="dossier-workspace__files-actions">
                        <label class="btn btn-primary btn-sm dossier-workspace__add">
                            <LucideIcon icon="Plus" :size="15" />
                            <span>{{ $t("contextDossiers.addFiles") }}</span>
                            <input ref="fileInput" type="file" multiple hidden accept=".pdf,.docx,.png,.jpg,.jpeg,.webp,.mp3,.wav,.m4a,.ogg" @change="addFiles" />
                        </label>
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm dossier-workspace__panel-toggle d-none d-lg-grid"
                            :title="$t('contextDossiers.hideFiles')"
                            :aria-label="$t('contextDossiers.hideFiles')"
                            @click="filesPanelCollapsed = true"
                        >
                            <LucideIcon icon="PanelLeftClose" :size="16" />
                        </button>
                    </div>
                </div>
                <div v-if="!dossier.files.length" class="dossier-workspace__empty">
                    <LucideIcon icon="FileUp" :size="30" />
                    <p>{{ $t("contextDossiers.noFiles") }}</p>
                </div>
                <ol v-else class="dossier-files">
                    <li v-for="(file, index) in orderedFiles" :key="file.id" :class="{ active: file.id === selectedFileId }">
                        <button class="dossier-files__select" @click="selectFile(file.id)">
                            <span class="dossier-files__format" :class="`dossier-files__format--${file.format}`"><LucideIcon :icon="formatIcon(file.format)" :size="16" /></span>
                            <span class="dossier-files__content">
                                <strong>{{ file.name }}</strong>
                                <small>{{ fileStatusLabel(file.status) }}</small>
                                <span v-if="isTransient(file.status)" class="progress dossier-files__progress"><span class="progress-bar" :style="{ width: `${file.progress}%` }"></span></span>
                            </span>
                        </button>
                        <span class="dossier-files__order">
                            <button :disabled="index === 0" :title="$t('contextDossiers.moveUp')" :aria-label="$t('contextDossiers.moveUp')" @click="move(file.id, -1)"><LucideIcon icon="ChevronUp" :size="15" /></button>
                            <button :disabled="index === orderedFiles.length - 1" :title="$t('contextDossiers.moveDown')" :aria-label="$t('contextDossiers.moveDown')" @click="move(file.id, 1)"><LucideIcon icon="ChevronDown" :size="15" /></button>
                            <button class="dossier-files__remove" :title="$t('contextDossiers.removeFile')" :aria-label="$t('contextDossiers.removeFile')" @click="removeFile(file)"><LucideIcon icon="Trash2" :size="14" /></button>
                        </span>
                    </li>
                </ol>
            </aside>

            <section class="dossier-workspace__detail" :class="{ 'd-none d-lg-block': !mobileDetail && dossier.files.length }">
                <template v-if="selectedFile">
                    <div class="dossier-file__navigation">
                        <button class="btn btn-link d-lg-none p-0" @click="mobileDetail = false"><LucideIcon icon="ArrowLeft" :size="16" /> {{ $t("contextDossiers.filesTitle") }}</button>
                        <button
                            v-if="filesPanelCollapsed"
                            type="button"
                            class="btn btn-outline-secondary btn-sm d-none d-lg-inline-flex"
                            @click="filesPanelCollapsed = false"
                        >
                            <LucideIcon icon="PanelLeftOpen" :size="16" />
                            {{ $t("contextDossiers.showFiles") }}
                        </button>
                    </div>
                    <div class="dossier-file__heading">
                        <div>
                            <input v-model="selectedFile.name" class="dossier-file__name" @change="persist" />
                            <p class="text-muted small mb-0">{{ formatBytes(selectedFile.size) }} · {{ selectedFile.format.toUpperCase() }}</p>
                        </div>
                        <div class="d-flex gap-1">
                            <button v-if="selectedFile.status === 'failed'" class="btn btn-outline-primary btn-sm" @click="retrySelected"><LucideIcon icon="RefreshCw" :size="15" /> {{ $t("contextDossiers.retry") }}</button>
                            <button class="btn btn-outline-danger btn-sm" @click="removeSelected"><LucideIcon icon="Trash2" :size="15" /> {{ $t("common.delete") }}</button>
                        </div>
                    </div>

                    <div v-if="selectedFile.status === 'failed'" class="alert alert-danger small mt-3 mb-3">
                        <LucideIcon icon="CircleX" :size="16" /> {{ selectedFile.failureMessage }}
                    </div>
                    <div v-else-if="isTransient(selectedFile.status)" class="dossier-processing">
                        <LucideIcon icon="LoaderCircle" :size="28" class="dossier-processing__spinner" />
                        <strong>{{ fileStatusLabel(selectedFile.status) }}</strong>
                        <small>{{ $t("contextDossiers.simulatedProcessing") }}</small>
                    </div>
                    <template v-else>
                        <div class="dossier-file__classification">
                            <div class="dossier-file__suggestion">
                                <span class="text-muted small">{{ $t("contextDossiers.suggestion") }}</span>
                                <strong>{{ typeName(selectedFile.suggestedTypeId) }}</strong>
                                <small>{{ $t("contextDossiers.confidence", { value: selectedFile.confidence }) }}</small>
                            </div>
                            <div class="flex-grow-1">
                                <label for="document-type" class="form-label small">{{ $t("contextDossiers.documentType") }}</label>
                                <select id="document-type" v-model="selectedFile.confirmedTypeId" class="form-select form-select-sm" @change="onTypeChanged">
                                    <option :value="null">{{ $t("contextDossiers.selectType") }}</option>
                                    <optgroup v-for="group in typeGroups" :key="group.key" :label="$t(`contextDossiers.typeGroups.${group.key}`)">
                                        <option v-for="type in group.types" :key="type.id" :value="type.id">{{ type.name }}</option>
                                    </optgroup>
                                </select>
                            </div>
                            <button v-if="selectedFile.status === 'review'" type="button" class="btn btn-outline-primary btn-sm align-self-end" :disabled="!selectedFile.confirmedTypeId" @click="confirmSelected">
                                <LucideIcon icon="Check" :size="15" /> {{ $t("contextDossiers.confirm") }}
                            </button>
                        </div>
                        <label for="file-transcript" class="form-label mt-3">{{ $t("contextDossiers.transcript") }}</label>
                        <textarea id="file-transcript" v-model="selectedFile.transcript" class="form-control dossier-file__transcript" @change="persist"></textarea>
                    </template>
                </template>
                <div v-else class="dossier-workspace__detail-empty">
                    <LucideIcon icon="Files" :size="34" />
                    <p>{{ $t("contextDossiers.selectFile") }}</p>
                </div>
            </section>
        </div>

        <section v-if="latestVersion" class="context-preview">
            <div class="context-preview__header">
                <div>
                    <div class="context-preview__eyebrow"><LucideIcon icon="BookOpenText" :size="15" /> {{ $t("contextDossiers.contextWorkspace") }}</div>
                    <h6 class="mb-1">{{ $t("contextDossiers.contextTitle") }} · v{{ latestVersion.version }}</h6>
                    <small class="text-muted">{{ formatDate(latestVersion.createdAt) }} · {{ $t("contextDossiers.filesCount", { count: latestVersion.variables.files.length }) }}</small>
                </div>
                <div v-if="previewTab === 'text' || previewTab === 'transcripts'" class="d-flex gap-2">
                    <button class="btn btn-outline-secondary btn-sm" @click="copyPreviewContent"><LucideIcon icon="Copy" :size="15" /> {{ $t("common.copy") }}</button>
                    <button class="btn btn-outline-secondary btn-sm" @click="downloadPreviewContent"><LucideIcon icon="Download" :size="15" /> {{ $t("contextDossiers.download") }}</button>
                </div>
            </div>
            <div v-if="dossierStatus === 'stale'" class="alert alert-warning small"><LucideIcon icon="RefreshCw" :size="16" /> {{ $t("contextDossiers.staleNotice") }}</div>
            <nav class="context-preview__tabs" role="tablist" :aria-label="$t('contextDossiers.contextWorkspace')">
                <button role="tab" :aria-selected="previewTab === 'text'" :class="{ active: previewTab === 'text' }" @click="previewTab = 'text'"><LucideIcon icon="AlignLeft" :size="15" />{{ $t("contextDossiers.fullTranscript") }}</button>
                <button role="tab" :aria-selected="previewTab === 'transcripts'" :class="{ active: previewTab === 'transcripts' }" @click="openIndividualTranscripts"><LucideIcon icon="Files" :size="15" />{{ $t("contextDossiers.individualTranscripts") }} ({{ latestVersion.variables.files.length }})</button>
                <button role="tab" :aria-selected="previewTab === 'variables'" :class="{ active: previewTab === 'variables' }" @click="previewTab = 'variables'"><LucideIcon icon="Braces" :size="15" />{{ $t("contextDossiers.variables") }}</button>
                <button role="tab" :aria-selected="previewTab === 'versions'" :class="{ active: previewTab === 'versions' }" @click="previewTab = 'versions'"><LucideIcon icon="History" :size="15" />{{ $t("contextDossiers.versions") }} ({{ dossier.preparedVersions.length }})</button>
                <button role="tab" :aria-selected="previewTab === 'history'" :class="{ active: previewTab === 'history' }" @click="previewTab = 'history'"><LucideIcon icon="Send" :size="15" />{{ $t("contextDossiers.history") }} ({{ dossier.dispatches.length }})</button>
            </nav>
            <pre v-if="previewTab === 'text'" class="context-preview__text">{{ latestVersion.content }}</pre>
            <div v-else-if="previewTab === 'transcripts'" class="context-transcripts">
                <aside class="context-transcripts__files" :aria-label="$t('contextDossiers.individualTranscripts')">
                    <button
                        v-for="item in contextTranscriptFiles"
                        :key="item.fileId"
                        type="button"
                        :class="{ active: contextTranscriptFileId === item.fileId }"
                        @click="contextTranscriptFileId = item.fileId"
                    >
                        <span class="context-transcripts__icon"><LucideIcon :icon="formatIcon(item.format)" :size="16" /></span>
                        <span><strong>{{ item.name }}</strong><code>{{ item.alias }}</code></span>
                        <LucideIcon icon="ChevronRight" :size="15" />
                    </button>
                </aside>
                <article v-if="activeContextTranscript" class="context-transcripts__reader">
                    <header>
                        <div>
                            <div class="context-transcripts__meta">
                                <span>{{ activeContextTranscript.format.toUpperCase() }}</span>
                                <span>{{ activeContextTranscript.typeName }}</span>
                            </div>
                            <h6>{{ activeContextTranscript.name }}</h6>
                            <code>{{ activeContextTranscript.alias }}</code>
                        </div>
                        <span class="context-transcripts__snapshot"><LucideIcon icon="LockKeyhole" :size="13" />{{ $t("contextDossiers.versionSnapshot", { version: latestVersion.version }) }}</span>
                    </header>
                    <pre>{{ activeContextTranscript.value }}</pre>
                </article>
            </div>
            <div v-else-if="previewTab === 'variables'" class="context-preview__variables">
                <code>{{ latestVersion.variables.consolidated }}</code>
                <div v-for="variable in latestVersion.variables.files" :key="variable.fileId"><code>{{ variable.alias }}</code><span>{{ variable.name }}</span></div>
            </div>
            <div v-else-if="previewTab === 'versions'" class="context-versions">
                <div class="context-versions__toolbar">
                    <div>
                        <label for="comparison-base-version" class="form-label">{{ $t("contextDossiers.compareFrom") }}</label>
                        <select id="comparison-base-version" v-model.number="comparisonBaseVersion" class="form-select form-select-sm">
                            <option v-for="version in sortedVersions" :key="`base-${version.version}`" :value="version.version">v{{ version.version }} · {{ formatDate(version.createdAt) }}</option>
                        </select>
                    </div>
                    <LucideIcon icon="ArrowRight" :size="18" class="context-versions__arrow" />
                    <div>
                        <label for="comparison-target-version" class="form-label">{{ $t("contextDossiers.compareTo") }}</label>
                        <select id="comparison-target-version" v-model.number="comparisonTargetVersion" class="form-select form-select-sm">
                            <option v-for="version in sortedVersions" :key="`target-${version.version}`" :value="version.version">v{{ version.version }} · {{ formatDate(version.createdAt) }}</option>
                        </select>
                    </div>
                    <div class="context-versions__file-select">
                        <label for="comparison-file" class="form-label">{{ $t("contextDossiers.compareFile") }}</label>
                        <select id="comparison-file" v-model="comparisonFileId" class="form-select form-select-sm">
                            <option v-for="file in comparableFiles" :key="file.fileId" :value="file.fileId">{{ file.name }}</option>
                        </select>
                    </div>
                </div>
                <div v-if="comparisonFileId" class="context-versions__summary" :class="comparisonChanged ? 'context-versions__summary--changed' : 'context-versions__summary--same'">
                    <LucideIcon :icon="comparisonChanged ? 'Diff' : 'CircleCheck'" :size="16" />
                    {{ $t(comparisonChanged ? "contextDossiers.transcriptChanged" : "contextDossiers.transcriptUnchanged") }}
                </div>
                <div v-if="comparisonFileId" class="context-versions__comparison">
                    <article>
                        <header><strong>v{{ comparisonBaseVersion }}</strong><span>{{ comparisonBaseVariable?.name || $t("contextDossiers.fileNotInVersion") }}</span></header>
                        <div class="context-versions__lines">
                            <div v-for="(line, index) in transcriptComparison.baseLines" :key="`base-line-${index}`" :class="{ changed: line.changed }"><span>{{ index + 1 }}</span><code>{{ line.value || " " }}</code></div>
                        </div>
                    </article>
                    <article>
                        <header><strong>v{{ comparisonTargetVersion }}</strong><span>{{ comparisonTargetVariable?.name || $t("contextDossiers.fileNotInVersion") }}</span></header>
                        <div class="context-versions__lines">
                            <div v-for="(line, index) in transcriptComparison.targetLines" :key="`target-line-${index}`" :class="{ changed: line.changed }"><span>{{ index + 1 }}</span><code>{{ line.value || " " }}</code></div>
                        </div>
                    </article>
                </div>
                <div v-else class="context-versions__empty">{{ $t("contextDossiers.noComparableFiles") }}</div>
            </div>
            <div v-else class="context-preview__history">
                <div v-for="dispatch in dossier.dispatches" :key="dispatch.id"><LucideIcon icon="CircleCheck" :size="16" /><span><strong>{{ dispatch.workflowName }}</strong><small>v{{ dispatch.version }} · {{ formatDate(dispatch.createdAt) }} · {{ dispatchScopeLabel(dispatch) }}</small><code v-for="alias in dispatch.variableAliases || []" :key="alias">{{ alias }}</code></span></div>
                <p v-if="!dossier.dispatches.length" class="text-muted mb-0">{{ $t("contextDossiers.noHistory") }}</p>
            </div>
        </section>

        <ModalComponent ref="dispatchModal" id="dispatch-context-dossier" title="contextDossiers.dispatchTitle" save-text="contextDossiers.send" :save-disabled="!canDispatch" @save="dispatch">
            <div class="alert alert-primary small"><LucideIcon icon="FlaskConical" :size="16" /> {{ $t("contextDossiers.dispatchNotice") }}</div>
            <label for="workflow-select" class="form-label">{{ $t("contextDossiers.workflow") }}</label>
            <select id="workflow-select" v-model="selectedWorkflowId" class="form-select">
                <option value="">{{ $t("contextDossiers.selectWorkflow") }}</option>
                <option v-for="workflow in workflows" :key="workflow.id" :value="workflow.id">{{ workflow.name }}</option>
            </select>
            <fieldset class="dispatch-scope">
                <legend>{{ $t("contextDossiers.dispatchContent") }}</legend>
                <label class="dispatch-scope__option" :class="{ active: dispatchMode === 'full' }">
                    <input v-model="dispatchMode" type="radio" value="full" />
                    <span><LucideIcon icon="Files" :size="17" /><strong>{{ $t("contextDossiers.dispatchFull") }}</strong><small>{{ $t("contextDossiers.dispatchFullHint") }}</small></span>
                </label>
                <label class="dispatch-scope__option" :class="{ active: dispatchMode === 'selected' }">
                    <input v-model="dispatchMode" type="radio" value="selected" />
                    <span><LucideIcon icon="ListChecks" :size="17" /><strong>{{ $t("contextDossiers.dispatchSelected") }}</strong><small>{{ $t("contextDossiers.dispatchSelectedHint") }}</small></span>
                </label>
            </fieldset>
            <div v-if="dispatchMode === 'selected'" class="dispatch-variables">
                <div class="dispatch-variables__header">
                    <span>{{ $t("contextDossiers.selectVariables") }}</span>
                    <button type="button" class="btn btn-link btn-sm" @click="toggleAllDispatchVariables">{{ allDispatchVariablesSelected ? $t("common.clearSelection") : $t("common.selectAll") }}</button>
                </div>
                <label v-for="variable in latestVersion.variables.files" :key="variable.fileId" class="dispatch-variables__item">
                    <input v-model="selectedDispatchFileIds" type="checkbox" :value="variable.fileId" />
                    <span><code>{{ variable.alias }}</code><small>{{ variable.name }}</small></span>
                </label>
                <p v-if="!selectedDispatchFileIds.length" class="text-danger small mb-0">{{ $t("contextDossiers.selectAtLeastOneVariable") }}</p>
            </div>
        </ModalComponent>
        <ConfirmModal
            ref="removeFileModal"
            id="remove-context-dossier-file"
            title="contextDossiers.removeFileTitle"
            message="contextDossiers.removeFileConfirm"
            :message-params="{ name: pendingRemovalFile?.name || '' }"
            confirm-text="common.delete"
            confirm-variant="danger"
            :is-loading="false"
            @confirm="confirmFileRemoval"
            @cancel="cancelFileRemoval"
        />
    </main>
    <main v-else class="dossier-not-found"><LucideIcon icon="FolderX" :size="38" /><h5>{{ $t("contextDossiers.notFound") }}</h5><button class="btn btn-primary" @click="back">{{ $t("common.back") }}</button></main>
</template>

<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import {
        FILE_STATUS,
        applySimulationStep,
        buildMockFile,
        deriveDossierStatus,
        dispatchDossier,
        findDossier,
        loadDocumentTypes,
        prepareDossierContext,
        retryMockFile,
        saveDossier,
    } from "@/services/documents/contextDossierStorage";

    export default {
        name: "ContextDossierWorkspace",
        components: { ModalComponent, ConfirmModal },
        data() { return { dossier: null, types: [], selectedFileId: null, pendingRemovalFile: null, timers: [], mobileDetail: false, filesPanelCollapsed: false, previewTab: "text", contextTranscriptFileId: "", workflows: [], selectedWorkflowId: "", dispatchMode: "full", selectedDispatchFileIds: [], comparisonBaseVersion: null, comparisonTargetVersion: null, comparisonFileId: "" }; },
        computed: {
            orderedFiles() { return [...(this.dossier?.files || [])].sort((a, b) => a.order - b.order); },
            selectedFile() { return this.dossier?.files.find((file) => file.id === this.selectedFileId) || null; },
            dossierStatus() { return this.dossier ? deriveDossierStatus(this.dossier) : "draft"; },
            canPrepare() { return this.dossier?.files.length > 0 && this.dossier.files.every((file) => file.status === FILE_STATUS.READY && file.confirmedTypeId); },
            latestVersion() { return this.dossier?.preparedVersions.find((item) => item.version === this.dossier.currentVersion) || null; },
            contextTranscriptFiles() {
                return (this.latestVersion?.variables.files || []).map((variable) => {
                    const file = this.dossier.files.find((item) => item.id === variable.fileId);
                    return {
                        ...variable,
                        format: file?.format || this.formatFromName(variable.name),
                        typeName: this.typeName(file?.confirmedTypeId),
                    };
                });
            },
            activeContextTranscript() { return this.contextTranscriptFiles.find((item) => item.fileId === this.contextTranscriptFileId) || this.contextTranscriptFiles[0] || null; },
            previewContent() { return this.previewTab === "transcripts" ? this.activeContextTranscript?.value || "" : this.latestVersion?.content || ""; },
            typeGroups() { return ["legal", "financial", "other"].map((key) => ({ key, types: this.types.filter((type) => type.group === key && type.active) })); },
            sortedVersions() { return [...(this.dossier?.preparedVersions || [])].sort((first, second) => first.version - second.version); },
            comparisonBaseSnapshot() { return this.sortedVersions.find((item) => item.version === this.comparisonBaseVersion) || null; },
            comparisonTargetSnapshot() { return this.sortedVersions.find((item) => item.version === this.comparisonTargetVersion) || null; },
            comparableFiles() {
                const variables = [...(this.comparisonBaseSnapshot?.variables.files || []), ...(this.comparisonTargetSnapshot?.variables.files || [])];
                return [...new Map(variables.map((item) => [item.fileId, item])).values()];
            },
            comparisonBaseVariable() { return this.comparisonBaseSnapshot?.variables.files.find((item) => item.fileId === this.comparisonFileId) || null; },
            comparisonTargetVariable() { return this.comparisonTargetSnapshot?.variables.files.find((item) => item.fileId === this.comparisonFileId) || null; },
            transcriptComparison() { return this.buildTranscriptComparison(this.comparisonBaseVariable?.value || "", this.comparisonTargetVariable?.value || ""); },
            comparisonChanged() { return (this.comparisonBaseVariable?.value || "") !== (this.comparisonTargetVariable?.value || ""); },
            allDispatchVariablesSelected() { return this.latestVersion?.variables.files.length > 0 && this.selectedDispatchFileIds.length === this.latestVersion.variables.files.length; },
            canDispatch() { return Boolean(this.selectedWorkflowId) && (this.dispatchMode === "full" || this.selectedDispatchFileIds.length > 0); },
        },
        mounted() { this.load(); this.loadWorkflows(); },
        beforeUnmount() { this.timers.forEach(clearTimeout); },
        methods: {
            load() { this.dossier = findDossier(this.$route.params.id); this.types = loadDocumentTypes(); this.selectedFileId = this.dossier?.files[0]?.id || null; this.initializeVersionComparison(); },
            back() { this.$router.push({ name: "Documents", query: { tab: "context-dossiers" } }); },
            persist() { this.dossier = saveDossier(this.dossier); },
            selectFile(id) { this.selectedFileId = id; this.mobileDetail = true; },
            formatIcon(format) { return { pdf: "FileText", image: "Image", docx: "FileType2", audio: "AudioLines" }[format] || "File"; },
            formatFromName(name) { const extension = name.split(".").pop()?.toLowerCase(); if (["png", "jpg", "jpeg", "webp"].includes(extension)) return "image"; if (["mp3", "wav", "m4a", "ogg"].includes(extension)) return "audio"; if (extension === "docx") return "docx"; return "pdf"; },
            fileStatusLabel(status) { return this.$t(`contextDossiers.fileStatus.${status}`); },
            isTransient(status) { return ["queued", "uploading", "transcribing", "classifying"].includes(status); },
            formatBytes(bytes) { return new Intl.NumberFormat(this.$i18n.locale, { style: "unit", unit: "megabyte", maximumFractionDigits: 1 }).format(bytes / 1048576); },
            formatDate(value) { return new Intl.DateTimeFormat(this.$i18n.locale, { dateStyle: "short", timeStyle: "short" }).format(new Date(value)); },
            typeName(id) { return this.types.find((type) => type.id === id)?.name || this.$t("contextDossiers.unidentified"); },
            addFiles(event) {
                const startOrder = this.dossier.files.length;
                const additions = [...event.target.files].map((file, index) => buildMockFile(file, startOrder + index + 1));
                this.dossier.files.push(...additions); this.persist(); event.target.value = "";
                additions.forEach((file, index) => this.simulate(file.id, index * 150));
                if (additions[0]) this.selectFile(additions[0].id);
            },
            simulate(id, delay = 0) {
                const steps = [FILE_STATUS.UPLOADING, FILE_STATUS.TRANSCRIBING, FILE_STATUS.CLASSIFYING, FILE_STATUS.REVIEW];
                steps.forEach((status, index) => this.timers.push(setTimeout(() => {
                    const fileIndex = this.dossier.files.findIndex((file) => file.id === id);
                    if (fileIndex < 0) return;
                    this.dossier.files.splice(fileIndex, 1, applySimulationStep(this.dossier.files[fileIndex], status)); this.persist();
                }, delay + index * 650)));
            },
            retrySelected() { const index = this.dossier.files.findIndex((file) => file.id === this.selectedFileId); this.dossier.files.splice(index, 1, retryMockFile(this.selectedFile)); this.persist(); this.simulate(this.selectedFileId); },
            confirmSelected() { this.selectedFile.status = FILE_STATUS.READY; this.persist(); },
            onTypeChanged() { this.persist(); },
            removeSelected() { if (this.selectedFile) this.removeFile(this.selectedFile); },
            removeFile(file) {
                this.pendingRemovalFile = file;
                this.$refs.removeFileModal?.open();
            },
            confirmFileRemoval() {
                const file = this.pendingRemovalFile;
                if (!file) return;
                const orderedIndex = this.orderedFiles.findIndex((item) => item.id === file.id);
                this.dossier.files = this.dossier.files.filter((item) => item.id !== file.id);
                this.dossier.files.sort((first, second) => first.order - second.order);
                this.dossier.files.forEach((item, order) => { item.order = order + 1; });
                if (this.selectedFileId === file.id) {
                    const nextIndex = Math.min(orderedIndex, this.dossier.files.length - 1);
                    this.selectedFileId = this.dossier.files[nextIndex]?.id || null;
                    if (!this.selectedFileId) this.mobileDetail = false;
                }
                this.persist();
                this.$refs.removeFileModal?.close();
                this.pendingRemovalFile = null;
                this.$notify({ title: "contextDossiers.title", message: "contextDossiers.fileRemoved", variant: "success", icon: "Trash2" });
            },
            cancelFileRemoval() { this.pendingRemovalFile = null; },
            move(id, direction) { const files = this.orderedFiles; const index = files.findIndex((file) => file.id === id); const target = index + direction; if (target < 0 || target >= files.length) return; [files[index].order, files[target].order] = [files[target].order, files[index].order]; this.persist(); },
            prepare() { this.dossier = prepareDossierContext(this.dossier); this.initializeVersionComparison(); this.previewTab = "text"; this.$notify({ title: "contextDossiers.title", message: "contextDossiers.preparedMessage", variant: "success", icon: "Check" }); },
            openIndividualTranscripts() { this.previewTab = "transcripts"; if (!this.contextTranscriptFileId) this.contextTranscriptFileId = this.contextTranscriptFiles[0]?.fileId || ""; },
            async copyPreviewContent() { await navigator.clipboard.writeText(this.previewContent); this.$notify({ title: "contextDossiers.title", message: "contextDossiers.copied", variant: "primary", icon: "Copy" }); },
            downloadPreviewContent() { const url = URL.createObjectURL(new Blob([this.previewContent], { type: "text/plain;charset=utf-8" })); const link = document.createElement("a"); const suffix = this.previewTab === "transcripts" ? this.activeContextTranscript?.name.replace(/\.[^.]+$/, "") : `contexto-v${this.latestVersion.version}`; link.href = url; link.download = `${suffix}.txt`; link.click(); URL.revokeObjectURL(url); },
            openDispatch() { this.selectedWorkflowId = ""; this.dispatchMode = "full"; this.selectedDispatchFileIds = this.latestVersion.variables.files.map((item) => item.fileId); this.$refs.dispatchModal.open(); },
            dispatch() { const workflow = this.workflows.find((item) => String(item.id) === String(this.selectedWorkflowId)); if (!workflow || (this.dispatchMode === "selected" && !this.selectedDispatchFileIds.length)) return; this.dossier = dispatchDossier(this.dossier, workflow, { mode: this.dispatchMode, fileIds: this.selectedDispatchFileIds }); this.$refs.dispatchModal.close(); this.previewTab = "history"; this.$notify({ title: "contextDossiers.title", message: "contextDossiers.dispatched", variant: "success", icon: "Send" }); },
            toggleAllDispatchVariables() { this.selectedDispatchFileIds = this.allDispatchVariablesSelected ? [] : this.latestVersion.variables.files.map((item) => item.fileId); },
            dispatchScopeLabel(dispatch) { return dispatch.mode === "selected" ? this.$t("contextDossiers.selectedTranscriptsCount", { count: dispatch.fileIds?.length || 0 }) : this.$t("contextDossiers.fullContext"); },
            initializeVersionComparison() { const versions = this.sortedVersions; this.comparisonTargetVersion = versions.at(-1)?.version || null; this.comparisonBaseVersion = versions.at(-2)?.version || versions.at(-1)?.version || null; this.$nextTick(() => { this.comparisonFileId = this.comparableFiles[0]?.fileId || ""; }); },
            buildTranscriptComparison(baseText, targetText) { const baseValues = baseText.split("\n"); const targetValues = targetText.split("\n"); const length = Math.max(baseValues.length, targetValues.length); const baseLines = []; const targetLines = []; for (let index = 0; index < length; index += 1) { const baseValue = baseValues[index] || ""; const targetValue = targetValues[index] || ""; const changed = baseValue !== targetValue; baseLines.push({ value: baseValue, changed }); targetLines.push({ value: targetValue, changed }); } return { baseLines, targetLines }; },
            async loadWorkflows() { const response = await WorkflowService.getWorkflowList(this.$store.state.userProfile.login); this.workflows = Array.isArray(response) ? response.filter((item) => item.name) : []; },
        },
    };
</script>

<style scoped>
    .dossier-workspace { display: block; width: 100%; height: 100%; min-width: 0; min-height: 0; padding: 1rem 1rem 2rem; overflow-x: hidden; overflow-y: auto; overscroll-behavior: contain; scrollbar-gutter: stable; color: var(--color-body-content); }
    .dossier-workspace:focus { outline: none; }
    .dossier-workspace:focus-visible { outline: 2px solid var(--color-btn-outline-primary, #0d6efd); outline-offset: -2px; }
    .dossier-workspace__header, .dossier-workspace__files-header, .dossier-file__heading, .context-preview__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
    .dossier-workspace__header { position: sticky; top: 0; z-index: 10; align-items: center; margin: -1rem -1rem 1rem; padding: .85rem 1rem; border-bottom: 1px solid var(--color-border-form-control); background: var(--color-card-content); }
    .dossier-workspace__back { display: grid; width: 40px; height: 40px; flex: 0 0 auto; place-items: center; padding: 0; }
    .dossier-workspace__name, .dossier-file__name { min-width: min(520px, 60vw); border: 0; border-bottom: 1px solid transparent; background: transparent; color: var(--color-heading-title, var(--color-body-content)); font-weight: 700; }
    .dossier-workspace__name { font-size: 1.15rem; }
    .dossier-workspace__name:focus, .dossier-file__name:focus { outline: none; border-bottom-color: var(--color-btn-outline-primary); }
    .dossier-workspace__primary-actions { display: flex; gap: .5rem; flex: 0 0 auto; }
    .dossier-workspace__primary-actions .btn { min-height: 38px; }
    .dossiers-status { border: 1px solid var(--color-border-form-control); border-radius: 999px; padding: .15rem .5rem; font-size: .7rem; white-space: nowrap; }
    .dossiers-status--prepared, .dossiers-status--ready { color: var(--bs-success); }
    .dossiers-status--review, .dossiers-status--stale { color: var(--bs-warning-text-emphasis, #9a6700); }
    .dossiers-status--failed { color: var(--bs-danger); }
    .dossier-workspace__layout { display: grid; grid-template-columns: clamp(300px, 28vw, 360px) minmax(0, 1fr); width: 100%; min-width: 0; min-height: 560px; border: 1px solid var(--color-border-form-control); border-radius: 8px; overflow: hidden; background: var(--color-card-content); transition: grid-template-columns 180ms ease-out; }
    .dossier-workspace__layout--focus { grid-template-columns: minmax(0, 1fr); }
    .dossier-workspace__files { display: flex; flex-direction: column; min-width: 0; border-right: 1px solid var(--color-border-form-control); background: var(--color-bg-body-content); }
    .dossier-workspace__files-header { align-items: center; min-height: 68px; padding: .75rem .85rem; }
    .dossier-workspace__files-actions { display: flex; align-items: center; gap: .4rem; }
    .dossier-workspace__add { display: inline-flex; align-items: center; min-height: 38px; white-space: nowrap; }
    .dossier-workspace__panel-toggle { width: 38px; height: 38px; place-items: center; padding: 0; }
    .dossier-files { flex: 1; padding: 0; margin: 0; overflow-y: auto; list-style: none; }
    .dossier-files li { display: flex; align-items: stretch; min-height: 64px; border-top: 1px solid var(--color-border-form-control); transition: background-color 150ms ease-out; }
    .dossier-files li:hover { background: color-mix(in srgb, var(--color-btn-outline-primary, #0d6efd) 5%, transparent); }
    .dossier-files li.active { background: var(--color-card-content); box-shadow: inset 3px 0 var(--color-btn-outline-primary, #0d6efd); }
    .dossier-files__select { display: flex; align-items: center; gap: .7rem; flex: 1; min-width: 0; min-height: 44px; padding: .7rem .75rem; border: 0; background: transparent; color: inherit; text-align: left; }
    .dossier-files__format { display: grid; width: 36px; height: 36px; flex: 0 0 auto; place-items: center; border: 1px solid var(--color-border-form-control); border-radius: 6px; background: var(--color-card-content); color: var(--color-btn-outline-primary); }
    .dossier-files__format--audio { color: #a05a00; } .dossier-files__format--image { color: #16794b; } .dossier-files__format--docx { color: #2563a8; }
    .dossier-files__content { min-width: 0; flex: 1; } .dossier-files__content strong, .dossier-files__content small { display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .dossier-files__content strong { font-size: .82rem; line-height: 1.35; } .dossier-files__content small { margin-top: .15rem; color: var(--color-text-muted); font-size: .72rem; }
    .dossier-files__progress { height: 3px; margin-top: .3rem; }
    .dossier-files__order { display: grid; align-content: center; margin-right: .35rem; } .dossier-files__order button { display: grid; width: 36px; height: 30px; place-items: center; border: 0; border-radius: 4px; background: transparent; color: var(--color-text-muted); padding: 0; } .dossier-files__order button:not(:disabled):hover { background: var(--color-bg-body-content); color: var(--color-body-content); } .dossier-files__order .dossier-files__remove { color: var(--bs-danger, #dc3545); } .dossier-files__order .dossier-files__remove:hover { background: color-mix(in srgb, var(--bs-danger, #dc3545) 10%, transparent); color: var(--bs-danger, #dc3545); }
    .dossier-workspace__detail { min-width: 0; padding: 1.25rem 1.4rem; overflow: hidden; }
    .dossier-file__navigation { min-height: 32px; margin-bottom: .5rem; }
    .dossier-file__name { min-width: 220px; max-width: 55vw; font-size: 1rem; }
    .dossier-file__classification { display: grid; grid-template-columns: minmax(170px, .7fr) minmax(240px, 1.3fr) auto; align-items: end; gap: 1rem; padding: 1rem; margin-top: 1rem; border: 1px solid var(--color-border-form-control); border-radius: 8px; background: var(--color-bg-body-content); }
    .dossier-file__suggestion { display: grid; align-content: center; min-width: 0; }
    .dossier-file__transcript { min-height: 340px; width: 100%; resize: vertical; font-family: ui-monospace, Consolas, monospace; font-size: .84rem; line-height: 1.65; }
    .dossier-processing, .dossier-workspace__detail-empty, .dossier-workspace__empty, .dossier-not-found { display: grid; place-items: center; align-content: center; min-height: 300px; color: var(--color-text-muted); text-align: center; }
    .dossier-processing__spinner { animation: spin 1s linear infinite; }
    .context-preview { width: 100%; min-width: 0; margin-top: 1rem; border: 1px solid var(--color-border-form-control); border-radius: 8px; overflow: hidden; background: var(--color-card-content); }
    .context-preview__header { align-items: center; min-height: 82px; padding: .9rem 1rem; }
    .context-preview__eyebrow { display: inline-flex; align-items: center; gap: .35rem; margin-bottom: .25rem; color: var(--color-btn-outline-primary, #0d6efd); font-size: .68rem; font-weight: 700; text-transform: uppercase; }
    .context-preview .alert { margin: 0 .8rem .8rem; }
    .context-preview__tabs { display: flex; gap: .2rem; padding: .35rem .5rem 0; overflow-x: auto; border-top: 1px solid var(--color-border-form-control); border-bottom: 1px solid var(--color-border-form-control); background: var(--color-bg-body-content); }
    .context-preview__tabs button { display: inline-flex; align-items: center; gap: .4rem; min-height: 42px; padding: .6rem .8rem; border: 0; border-bottom: 2px solid transparent; background: transparent; color: var(--color-text-muted); font-size: .76rem; white-space: nowrap; }
    .context-preview__tabs button:hover { color: var(--color-body-content); }
    .context-preview__tabs button.active { border-bottom-color: var(--color-btn-outline-primary); background: var(--color-card-content); color: var(--color-body-content); font-weight: 600; }
    .context-preview__text { max-height: 420px; margin: 0; padding: 1rem; overflow: auto; background: var(--color-bg-body-content); color: var(--color-body-content); font-size: .78rem; white-space: pre-wrap; }
    .context-transcripts { display: grid; grid-template-columns: minmax(250px, 30%) minmax(0, 1fr); min-height: 430px; }
    .context-transcripts__files { overflow-y: auto; border-right: 1px solid var(--color-border-form-control); background: var(--color-bg-body-content); }
    .context-transcripts__files > button { display: grid; grid-template-columns: 34px minmax(0, 1fr) auto; align-items: center; gap: .65rem; width: 100%; min-height: 68px; padding: .65rem .75rem; border: 0; border-bottom: 1px solid var(--color-border-form-control); background: transparent; color: var(--color-body-content); text-align: left; }
    .context-transcripts__files > button:hover { background: color-mix(in srgb, var(--color-btn-outline-primary, #0d6efd) 5%, transparent); }
    .context-transcripts__files > button.active { background: var(--color-card-content); box-shadow: inset 3px 0 var(--color-btn-outline-primary, #0d6efd); }
    .context-transcripts__icon { display: grid; width: 34px; height: 34px; place-items: center; border: 1px solid var(--color-border-form-control); border-radius: 6px; color: var(--color-btn-outline-primary, #0d6efd); background: var(--color-card-content); }
    .context-transcripts__files > button > span:nth-child(2) { display: grid; min-width: 0; }
    .context-transcripts__files strong, .context-transcripts__files code { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .context-transcripts__files strong { font-size: .76rem; }
    .context-transcripts__files code { margin-top: .15rem; color: #d34076; font-size: .65rem; }
    .context-transcripts__reader { min-width: 0; background: var(--color-card-content); }
    .context-transcripts__reader > header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; min-height: 82px; padding: .85rem 1rem; border-bottom: 1px solid var(--color-border-form-control); }
    .context-transcripts__reader h6 { margin: .25rem 0 .15rem; }
    .context-transcripts__reader header code { color: #d34076; font-size: .68rem; }
    .context-transcripts__meta { display: flex; gap: .35rem; }
    .context-transcripts__meta span { padding: .12rem .4rem; border: 1px solid var(--color-border-form-control); border-radius: 999px; color: var(--color-text-muted); font-size: .62rem; font-weight: 600; text-transform: uppercase; }
    .context-transcripts__snapshot { display: inline-flex; align-items: center; gap: .3rem; flex: 0 0 auto; padding: .25rem .5rem; border-radius: 999px; background: var(--color-bg-body-content); color: var(--color-text-muted); font-size: .66rem; }
    .context-transcripts__reader > pre { min-height: 330px; max-height: 440px; margin: 0; padding: 1rem; overflow: auto; background: var(--color-bg-body-content); color: var(--color-body-content); font-family: ui-monospace, Consolas, monospace; font-size: .8rem; line-height: 1.65; white-space: pre-wrap; }
    .context-preview__variables, .context-preview__history { display: grid; gap: .5rem; padding: 1rem; }
    .context-preview__variables > div, .context-preview__history > div { display: flex; align-items: center; gap: .75rem; padding: .55rem; border: 1px solid var(--color-border-form-control); border-radius: 6px; }
    .context-preview__variables code { color: #d34076; } .context-preview__variables span, .context-preview__history small { color: var(--color-text-muted); font-size: .75rem; }
    .context-preview__history span, .context-preview__history small { display: block; }
    .context-preview__history span { min-width: 0; }
    .context-preview__history code { display: inline-block; margin: .3rem .35rem 0 0; color: #d34076; font-size: .7rem; }
    .context-versions { padding: 1rem; }
    .context-versions__toolbar { display: grid; grid-template-columns: minmax(190px, .7fr) auto minmax(190px, .7fr) minmax(240px, 1fr); align-items: end; gap: .75rem; margin-bottom: .85rem; }
    .context-versions__toolbar .form-label { margin-bottom: .3rem; color: var(--color-text-muted); font-size: .72rem; font-weight: 600; }
    .context-versions__arrow { align-self: end; margin-bottom: .45rem; color: var(--color-text-muted); }
    .context-versions__summary { display: flex; align-items: center; gap: .45rem; margin-bottom: .75rem; padding: .55rem .7rem; border: 1px solid var(--color-border-form-control); border-radius: 6px; font-size: .76rem; font-weight: 600; }
    .context-versions__summary--changed { border-left: 3px solid var(--bs-warning, #ffc107); }
    .context-versions__summary--same { border-left: 3px solid var(--bs-success, #198754); }
    .context-versions__comparison { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); border: 1px solid var(--color-border-form-control); border-radius: 8px; overflow: hidden; }
    .context-versions__comparison article { min-width: 0; background: var(--color-bg-body-content); }
    .context-versions__comparison article + article { border-left: 1px solid var(--color-border-form-control); }
    .context-versions__comparison article > header { display: flex; align-items: center; gap: .6rem; min-height: 48px; padding: .65rem .8rem; border-bottom: 1px solid var(--color-border-form-control); background: var(--color-card-content); }
    .context-versions__comparison article > header strong { display: grid; min-width: 32px; height: 24px; place-items: center; border-radius: 999px; background: var(--color-bg-body-content); font-size: .72rem; }
    .context-versions__comparison article > header span { overflow: hidden; color: var(--color-text-muted); font-size: .72rem; text-overflow: ellipsis; white-space: nowrap; }
    .context-versions__lines { max-height: 360px; overflow: auto; }
    .context-versions__lines > div { display: grid; grid-template-columns: 34px minmax(0, 1fr); min-height: 28px; border-bottom: 1px solid color-mix(in srgb, var(--color-border-form-control) 55%, transparent); }
    .context-versions__lines > div > span { padding: .35rem .45rem; color: var(--color-text-muted); font-family: ui-monospace, Consolas, monospace; font-size: .68rem; text-align: right; user-select: none; }
    .context-versions__lines code { padding: .35rem .55rem; color: var(--color-body-content); font-size: .72rem; line-height: 1.5; white-space: pre-wrap; word-break: break-word; }
    .context-versions__lines > div.changed { box-shadow: inset 3px 0 var(--bs-warning, #ffc107); }
    .context-versions__lines > div.changed code { background: color-mix(in srgb, var(--bs-warning, #ffc107) 12%, transparent); font-weight: 600; }
    .context-versions__empty { padding: 2rem; color: var(--color-text-muted); text-align: center; }
    .dispatch-scope { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .6rem; margin: 1rem 0 0; padding: 0; border: 0; }
    .dispatch-scope legend { grid-column: 1 / -1; margin-bottom: 0; font-size: .86rem; font-weight: 600; }
    .dispatch-scope__option { display: flex; gap: .6rem; min-height: 88px; padding: .75rem; border: 1px solid var(--color-border-form-control); border-radius: 8px; cursor: pointer; transition: border-color 150ms ease-out, background-color 150ms ease-out; }
    .dispatch-scope__option.active { border-color: var(--color-btn-outline-primary, #0d6efd); background: color-mix(in srgb, var(--color-btn-outline-primary, #0d6efd) 7%, transparent); }
    .dispatch-scope__option input { margin-top: .2rem; }
    .dispatch-scope__option > span { display: grid; grid-template-columns: auto 1fr; align-content: start; column-gap: .4rem; min-width: 0; }
    .dispatch-scope__option strong { font-size: .78rem; }
    .dispatch-scope__option small { grid-column: 1 / -1; margin-top: .35rem; color: var(--color-text-muted); font-size: .7rem; line-height: 1.45; }
    .dispatch-variables { max-height: 260px; margin-top: .75rem; overflow-y: auto; border: 1px solid var(--color-border-form-control); border-radius: 8px; }
    .dispatch-variables__header { position: sticky; top: 0; z-index: 1; display: flex; align-items: center; justify-content: space-between; padding: .45rem .7rem; border-bottom: 1px solid var(--color-border-form-control); background: var(--color-card-content); font-size: .75rem; font-weight: 600; }
    .dispatch-variables__item { display: flex; align-items: center; gap: .65rem; min-height: 52px; padding: .55rem .7rem; border-bottom: 1px solid var(--color-border-form-control); cursor: pointer; }
    .dispatch-variables__item:last-of-type { border-bottom: 0; }
    .dispatch-variables__item > span { display: grid; min-width: 0; }
    .dispatch-variables__item code { color: #d34076; font-size: .72rem; }
    .dispatch-variables__item small { overflow: hidden; color: var(--color-text-muted); font-size: .68rem; text-overflow: ellipsis; white-space: nowrap; }
    .dispatch-variables > p { padding: .5rem .7rem; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 1199px) { .dossier-workspace__layout { grid-template-columns: 300px minmax(0, 1fr); } .dossier-file__classification { grid-template-columns: 1fr; align-items: stretch; } .context-versions__toolbar { grid-template-columns: 1fr auto 1fr; } .context-versions__file-select { grid-column: 1 / -1; } }
    @media (max-width: 991px) { .dossier-workspace { padding: .75rem; } .dossier-workspace__header { margin: -.75rem -.75rem .75rem; padding: .75rem; } .dossier-workspace__layout, .dossier-workspace__layout--focus { display: block; min-height: 60vh; } .dossier-workspace__files { min-height: 60vh; border-right: 0; } .dossier-files li { min-height: 132px; } .dossier-files__order button { width: 44px; height: 44px; } .dossier-workspace__detail { padding: 1rem; } }
    @media (max-width: 650px) { .dossier-workspace__header { position: static; flex-direction: column; align-items: stretch; } .dossier-workspace__header > .d-flex:first-child { width: 100%; min-width: 0; } .dossier-workspace__header > .d-flex:first-child > div { flex: 1; min-width: 0; } .dossier-workspace__header > .d-flex:first-child > div > .d-flex { display: grid !important; grid-template-columns: minmax(0, 1fr); justify-items: start; gap: .35rem !important; } .dossier-workspace__primary-actions { width: 100%; } .dossier-workspace__primary-actions .btn { flex: 1; min-height: 44px; } .dossier-workspace__name { min-width: 0; width: 100%; } .dossier-file__navigation .btn, .dossier-file__heading .btn, .dossier-file__classification .btn, .dossier-file__classification .form-select { min-height: 44px; } .dossier-file__heading, .context-preview__header { flex-direction: column; } .context-preview__header > .d-flex { width: 100%; } .context-preview__header > .d-flex .btn { flex: 1; min-height: 44px; } .dossier-file__heading > .d-flex { width: 100%; } .dossier-file__heading > .d-flex .btn { flex: 1; } .dossier-file__classification { grid-template-columns: 1fr; } .dossier-file__suggestion { min-width: 0; } .context-preview__tabs { overflow-x: auto; } .context-preview__tabs button { min-height: 44px; white-space: nowrap; } .context-transcripts { grid-template-columns: 1fr; } .context-transcripts__files { display: flex; overflow-x: auto; overflow-y: hidden; border-right: 0; border-bottom: 1px solid var(--color-border-form-control); } .context-transcripts__files > button { min-width: 220px; border-right: 1px solid var(--color-border-form-control); border-bottom: 0; } .context-transcripts__reader > header { flex-direction: column; } .context-transcripts__snapshot { align-self: flex-start; } .context-versions__toolbar { grid-template-columns: 1fr; } .context-versions__arrow { display: none; } .context-versions__file-select { grid-column: auto; } .context-versions__comparison { grid-template-columns: 1fr; } .context-versions__comparison article + article { border-top: 1px solid var(--color-border-form-control); border-left: 0; } .dispatch-scope { grid-template-columns: 1fr; } .dispatch-scope__option, .dispatch-variables__item { min-height: 56px; } }
    @media (prefers-reduced-motion: reduce) { .dossier-workspace__layout, .dossier-files li { transition: none; } }
</style>

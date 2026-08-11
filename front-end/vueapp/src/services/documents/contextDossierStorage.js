const DOSSIERS_KEY = "woopi-context-dossiers";
const TYPES_KEY = "woopi-document-types";

export const DOSSIER_STATUS = {
    DRAFT: "draft",
    PROCESSING: "processing",
    REVIEW: "review",
    READY: "ready",
    PREPARED: "prepared",
    STALE: "stale",
    FAILED: "failed",
};

export const FILE_STATUS = {
    QUEUED: "queued",
    UPLOADING: "uploading",
    TRANSCRIBING: "transcribing",
    CLASSIFYING: "classifying",
    REVIEW: "review",
    READY: "ready",
    FAILED: "failed",
};

const DEFAULT_TYPES = [
    { id: "type-contract", group: "legal", name: "Contrato", active: true },
    { id: "type-power", group: "legal", name: "Procuração", active: true },
    { id: "type-invoice", group: "financial", name: "Nota fiscal", active: true },
    { id: "type-statement", group: "financial", name: "Demonstrativo financeiro", active: true },
    { id: "type-minutes", group: "other", name: "Ata de reunião", active: true },
    { id: "type-other", group: "other", name: "Não identificado", active: true },
];

const SAMPLE_TRANSCRIPTS = {
    pdf: "CONTRATO DE PRESTAÇÃO DE SERVIÇOS\n\nAs partes identificadas acordam as condições comerciais, os prazos de entrega e as responsabilidades descritas neste instrumento.",
    image: "NOTA FISCAL DE SERVIÇOS\nFornecedor: Exemplo Tecnologia Ltda.\nValor total: R$ 8.450,00\nVencimento: 15/08/2026.",
    docx: "DEMONSTRATIVO FINANCEIRO\n\nReceita operacional, despesas administrativas e projeções consolidadas para o período analisado.",
    audio: "[00:00] Participante 1: Vamos revisar os pontos jurídicos e financeiros.\n[00:08] Participante 2: O contrato está aprovado, condicionado à atualização dos valores.",
};

function createSampleFile(id, name, format, typeId, order, status = FILE_STATUS.READY) {
    return {
        id,
        name,
        mimeType: mimeFor(format),
        format,
        size: 1048576 * order,
        order,
        status,
        progress: status === FILE_STATUS.READY ? 100 : 72,
        suggestedTypeId: typeId,
        confirmedTypeId: status === FILE_STATUS.FAILED ? null : typeId,
        confidence: 88 - order,
        transcript: SAMPLE_TRANSCRIPTS[format],
        failureMessage: status === FILE_STATUS.FAILED ? "Não foi possível simular a transcrição." : "",
    };
}

function createSampleDossiers() {
    return [
        createReadySample(),
        createReviewSample(),
        createFailedSample(),
    ];
}

function createReadySample() {
    const files = [
        createSampleFile("file-contract", "contrato_social.pdf", "pdf", "type-contract", 1),
        createSampleFile("file-invoice", "nota_fiscal_agosto.jpg", "image", "type-invoice", 2),
        createSampleFile("file-report", "demonstrativo_2026.docx", "docx", "type-statement", 3),
        createSampleFile("file-meeting", "reuniao_diretoria.m4a", "audio", "type-minutes", 4),
    ];
    const dossier = baseDossier("dossier-ready", "Aquisição Empresa Horizonte", files);
    const snapshot = buildContextSnapshot(dossier, 1);
    return { ...dossier, preparedVersions: [snapshot], currentVersion: 1 };
}

function createReviewSample() {
    const files = [
        createSampleFile("file-review", "procuracao_diretoria.pdf", "pdf", "type-power", 1, FILE_STATUS.REVIEW),
    ];
    return baseDossier("dossier-review", "Revisão societária", files);
}

function createFailedSample() {
    const files = [
        createSampleFile("file-failed", "assembleia_extraordinaria.wav", "audio", "type-minutes", 1, FILE_STATUS.FAILED),
    ];
    return baseDossier("dossier-failed", "Assembleia de acionistas", files);
}

function baseDossier(id, name, files = []) {
    const now = new Date().toISOString();
    return {
        id,
        name,
        description: "",
        files,
        currentVersion: 0,
        preparedVersions: [],
        dispatches: [],
        createdAt: now,
        updatedAt: now,
    };
}

function read(key, fallback) {
    try {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : fallback;
    } catch {
        return fallback;
    }
}

function write(key, value) {
    localStorage.setItem(key, JSON.stringify(value));
    return value;
}

export function initializeContextDossiers() {
    if (!localStorage.getItem(DOSSIERS_KEY)) write(DOSSIERS_KEY, createSampleDossiers());
    if (!localStorage.getItem(TYPES_KEY)) write(TYPES_KEY, DEFAULT_TYPES);
}

export function loadDossiers() {
    initializeContextDossiers();
    return read(DOSSIERS_KEY, []).map(normalizeTransientFiles);
}

function normalizeTransientFiles(dossier) {
    const transient = [FILE_STATUS.QUEUED, FILE_STATUS.UPLOADING, FILE_STATUS.TRANSCRIBING, FILE_STATUS.CLASSIFYING];
    const files = dossier.files.map((file) => transient.includes(file.status)
        ? { ...file, status: FILE_STATUS.REVIEW, progress: 100 }
        : file);
    return { ...dossier, files };
}

export function findDossier(id) {
    return loadDossiers().find((dossier) => dossier.id === id) || null;
}

export function saveDossier(dossier) {
    const dossiers = loadDossiers();
    const updated = { ...dossier, updatedAt: new Date().toISOString() };
    const index = dossiers.findIndex((item) => item.id === updated.id);
    if (index >= 0) dossiers.splice(index, 1, updated);
    else dossiers.unshift(updated);
    write(DOSSIERS_KEY, dossiers);
    return updated;
}

export function createDossier({ name, description = "" }) {
    const dossier = baseDossier(crypto.randomUUID(), name.trim());
    dossier.description = description.trim();
    return saveDossier(dossier);
}

export function duplicateDossier(id) {
    const source = findDossier(id);
    if (!source) return null;
    const files = source.files.map((file) => ({ ...file, id: crypto.randomUUID() }));
    const copy = baseDossier(crypto.randomUUID(), `${source.name} - cópia`, files);
    return saveDossier(copy);
}

export function deleteDossier(id) {
    return write(DOSSIERS_KEY, loadDossiers().filter((dossier) => dossier.id !== id));
}

export function deriveDossierStatus(dossier) {
    if (hasTransientFiles(dossier.files)) return DOSSIER_STATUS.PROCESSING;
    if (dossier.files.some((file) => file.status === FILE_STATUS.FAILED)) return DOSSIER_STATUS.FAILED;
    if (dossier.files.some((file) => file.status === FILE_STATUS.REVIEW)) return DOSSIER_STATUS.REVIEW;
    if (!dossier.files.length) return DOSSIER_STATUS.DRAFT;
    if (isPreparedContentStale(dossier)) return DOSSIER_STATUS.STALE;
    if (dossier.currentVersion) return DOSSIER_STATUS.PREPARED;
    return DOSSIER_STATUS.READY;
}

function hasTransientFiles(files) {
    return files.some((file) => [FILE_STATUS.QUEUED, FILE_STATUS.UPLOADING, FILE_STATUS.TRANSCRIBING, FILE_STATUS.CLASSIFYING].includes(file.status));
}

function isPreparedContentStale(dossier) {
    if (!dossier.currentVersion) return false;
    const latest = dossier.preparedVersions.find((version) => version.version === dossier.currentVersion);
    return latest?.sourceHash !== buildSourceHash(dossier);
}

export function loadDocumentTypes() {
    initializeContextDossiers();
    return read(TYPES_KEY, DEFAULT_TYPES);
}

export function saveDocumentType(type) {
    const types = loadDocumentTypes();
    const entry = { ...type, id: type.id || crypto.randomUUID() };
    const index = types.findIndex((item) => item.id === entry.id);
    if (index >= 0) types.splice(index, 1, entry);
    else types.push(entry);
    write(TYPES_KEY, types);
    return entry;
}

export function deleteDocumentType(id) {
    return write(TYPES_KEY, loadDocumentTypes().filter((type) => type.id !== id));
}

export function resetContextDossierDemo() {
    write(DOSSIERS_KEY, createSampleDossiers());
    write(TYPES_KEY, DEFAULT_TYPES);
}

export function buildMockFile(file, order) {
    const format = resolveFormat(file);
    const typeId = suggestTypeId(file.name, format);
    return createSampleFile(crypto.randomUUID(), file.name, format, typeId, order, FILE_STATUS.QUEUED);
}

function resolveFormat(file) {
    const extension = file.name.split(".").pop()?.toLowerCase();
    if (["png", "jpg", "jpeg", "webp"].includes(extension)) return "image";
    if (["mp3", "wav", "m4a", "ogg"].includes(extension)) return "audio";
    if (extension === "docx") return "docx";
    return "pdf";
}

function mimeFor(format) {
    const mimeTypes = { pdf: "application/pdf", image: "image/jpeg", docx: "application/vnd.openxmlformats-officedocument.wordprocessingml.document", audio: "audio/mpeg" };
    return mimeTypes[format];
}

function suggestTypeId(name, format) {
    const normalized = name.toLowerCase();
    if (normalized.includes("nota") || normalized.includes("fatura")) return "type-invoice";
    if (normalized.includes("procur")) return "type-power";
    if (format === "audio") return "type-minutes";
    if (format === "docx") return "type-statement";
    return "type-contract";
}

export function applySimulationStep(file, status) {
    const progressByStatus = { uploading: 28, transcribing: 58, classifying: 82, review: 100 };
    return { ...file, status, progress: progressByStatus[status] || file.progress };
}

export function retryMockFile(file) {
    return { ...file, status: FILE_STATUS.QUEUED, progress: 0, failureMessage: "" };
}

export function prepareDossierContext(dossier) {
    const version = Math.max(0, ...dossier.preparedVersions.map((item) => item.version)) + 1;
    const snapshot = buildContextSnapshot(dossier, version);
    return saveDossier({ ...dossier, currentVersion: version, preparedVersions: [...dossier.preparedVersions, snapshot] });
}

function buildContextSnapshot(dossier, version) {
    return {
        id: crypto.randomUUID(),
        version,
        content: generateContextText(dossier, version),
        variables: generateVariables(dossier),
        sourceHash: buildSourceHash(dossier),
        createdAt: new Date().toISOString(),
    };
}

export function generateContextText(dossier, version) {
    const header = [`DOSSIÊ: ${dossier.name}`, `VERSÃO: ${version}`, `ARQUIVOS: ${dossier.files.length}`];
    const sections = orderedFiles(dossier).map((file, index) => buildFileSection(file, index));
    return [...header, "", ...sections].join("\n");
}

function buildFileSection(file, index) {
    const type = read(TYPES_KEY, DEFAULT_TYPES).find((item) => item.id === file.confirmedTypeId);
    return [`===== ARQUIVO ${index + 1} =====`, `Nome: ${file.name}`, `Formato: ${file.format.toUpperCase()}`, `Tipo documental: ${type?.name || "Não identificado"}`, "", file.transcript, ""].join("\n");
}

function orderedFiles(dossier) {
    return [...dossier.files].sort((first, second) => first.order - second.order);
}

function generateVariables(dossier) {
    const used = new Set();
    const entries = orderedFiles(dossier).map((file) => {
        const alias = uniqueAlias(slugify(file.name.replace(/\.[^.]+$/, "")), used);
        return { fileId: file.id, alias: `{{arquivos.${alias}}}`, name: file.name, value: file.transcript };
    });
    return { consolidated: "{{contexto}}", files: entries };
}

function uniqueAlias(base, used) {
    let alias = base || "arquivo";
    let suffix = 2;
    while (used.has(alias)) alias = `${base}_${suffix++}`;
    used.add(alias);
    return alias;
}

function slugify(value) {
    return value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase().replace(/[^a-z0-9]+/g, "_").replace(/^_|_$/g, "");
}

function buildSourceHash(dossier) {
    return JSON.stringify(orderedFiles(dossier).map((file) => [file.id, file.name, file.order, file.confirmedTypeId, file.transcript]));
}

export function dispatchDossier(dossier, workflow, selection = { mode: "full", fileIds: [] }) {
    const version = dossier.preparedVersions.find((item) => item.version === dossier.currentVersion);
    const selectedVariables = resolveDispatchVariables(version, selection);
    const dispatch = {
        id: crypto.randomUUID(),
        version: dossier.currentVersion,
        workflowId: workflow.id,
        workflowName: workflow.name,
        mode: selection.mode,
        fileIds: selectedVariables.map((item) => item.fileId),
        variableAliases: selectedVariables.map((item) => item.alias),
        content: buildDispatchContent(version, selection.mode, selectedVariables),
        createdAt: new Date().toISOString(),
        status: "sent",
    };
    return saveDossier({ ...dossier, dispatches: [dispatch, ...dossier.dispatches] });
}

function resolveDispatchVariables(version, selection) {
    if (selection.mode !== "selected") return version?.variables.files || [];
    const selectedIds = new Set(selection.fileIds);
    return (version?.variables.files || []).filter((item) => selectedIds.has(item.fileId));
}

function buildDispatchContent(version, mode, variables) {
    if (mode !== "selected") return version?.content || "";
    return variables.map((item) => [item.alias, `Arquivo: ${item.name}`, "", item.value].join("\n")).join("\n\n---\n\n");
}

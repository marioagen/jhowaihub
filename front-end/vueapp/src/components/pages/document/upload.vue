<template>
    <main class="flex-shrink-0" v-if="loading">
        <div class="container mb-5">
            <div class="row justify-content-md-center" style="height: 100%">
                <div class="col-md-auto">
                    <div class="div-center">
                        <div>
                            <div class="mb-3" style="width: 100%; float: left">
                                <h5 class="h5-custom-modal">{{ message }}</h5>
                            </div>
                            <div style="text-align: center">
                                <img
                                    svg-inline
                                    src="@/assets/img/icon-load-circle.svg"
                                    alt="Loading"
                                    width="60"
                                    class="refresh-animated"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
    <main v-if="!loading">
        <div class="container-fluid mt-4">
            <div class="custom-padding">
                <div class="row">
                    <breadcrumb :crumbs="crumbsData" />
                </div>
                <div class="row" style="max-height: calc(100% - 70px); overflow-y: auto">
                    <div class="col-md-12">
                        <form class="form-upload" @submit.prevent="save">
                            <h5 class="mb-4">{{ $t('labelGeneralInformation') }}</h5>
                            <div class="col-lg-12 col-md-12 col-sm-12 mb-3">
                                <label class="label-container">
                                    {{ $t('labelUploadPdf') }}
                                    <span class="clear-button">
                                        <img
                                            src="../../../assets/img/icon-dropzone-remove-all.svg"
                                            alt="Remove All"
                                            :title="$t('labelRemoveAllDropzone')"
                                            @click="confirmationDialog()"
                                        />
                                    </span>
                                </label>
                                <div
                                    action="/file-upload"
                                    class="dropzone"
                                    id="dropzoneUpload"
                                    ref="dropzone"
                                ></div>
                            </div>
                            <div class="mb-1">
                                <label class="form-label" for="descId">
                                    {{ $t('labelDescriptionDocumentNote') }}
                                </label>
                                <textarea
                                    rows="5"
                                    id="descId"
                                    v-validate
                                    name="text"
                                    v-model="form.description"
                                    required
                                    :class="{
                                        'form-control': true,
                                        'red-warning': form.description.length > 250,
                                    }"
                                >
></textarea
                                >
                            </div>
                            <div class="mb-0">
                                <div v-if="form.description.length <= 250">
                                    <a
                                        style="
                                            text-decoration: none;
                                            color: #aeb2ba;
                                            cursor: default;
                                        "
                                    >
                                        {{ 250 - form.description.length }}
                                        {{
                                            250 - form.description.length > 1
                                                ? $t('labelCharacters')
                                                : $t('labelCharacter')
                                        }}
                                    </a>
                                </div>
                                <div v-if="form.description.length > 250">
                                    <a
                                        style="
                                            text-decoration: none;
                                            color: #dc3545;
                                            cursor: default;
                                        "
                                    >
                                        {{ 250 - form.description.length }}
                                        {{ $t('labelCharacters') }}
                                    </a>
                                    <a
                                        class="exceedDesc"
                                        style="
                                            float: right;
                                            text-decoration: none;
                                            color: #dc3545;
                                            cursor: default;
                                        "
                                    >
                                        {{ $t('labelExceedDesc') }}
                                    </a>
                                </div>
                            </div>
                            <button
                                type="submit"
                                class="btn btn-primary m-2"
                                :title="$t('labelSend')"
                                v-if="form.description.length > 250"
                                disabled
                                style="float: right"
                            >
                                {{ $t('labelSend') }}
                            </button>
                            <button
                                type="submit"
                                class="btn btn-primary m-2"
                                :title="$t('labelSend')"
                                v-if="form.description.length <= 250"
                                style="float: right"
                            >
                                {{ $t('labelSend') }}
                            </button>
                            <router-link
                                class="btn btn-secondary m-2 btn-custom-cancel"
                                style="float: right"
                                :to="{ name: 'DocumentList', query: { page: '1' } }"
                                :title="$t('labelCancel')"
                            >
                                {{ $t('labelCancel') }}
                            </router-link>
                        </form>
                    </div>
                    <div class="col-md-6"></div>
                </div>
            </div>
        </div>
        <!-- Component ToastAlert -->
        <toast-alert
            :showToast="toastShow"
            :colorToast="toastColor"
            :messageToast="toastMessage"
            @close="closeToast"
        />
        <modal-alert
            v-if="modalAlertShow"
            :type="'Confirm'"
            :alertTitle="$t('labelRemoveAllFilesDropzone')"
            :alertMessage="$t('labelThisActionRemoveAllFiles')"
            :okLabel="$t('labelConfirm')"
            :cancelLabel="$t('labelCancel')"
            @open="removeAllFiles"
            @close="closeModal"
        />
    </main>
</template>

<script>
    import NavBar from '@/components/common/nav-bar'
    import Breadcrumb from '@/components/common/breadcrumb'
    import ModalAlert from '@/components/common/modal-alert'
    import ToastAlert from '@/components/common/toast-alert'
    import api from '@/services/api'
    import uploadFileWorker from '@/workers'
    import Dropzone from 'dropzone'
    import 'dropzone/dist/dropzone.css'

    export default {
        name: 'DocumentUpload',
        directives: {
            validate: {
                inserted: function (el, binding) {
                    el.addEventListener('input', function () {
                        el.setCustomValidity('')
                        if (!el.checkValidity()) {
                            el.reportValidity()
                        }
                    })

                    el.addEventListener('invalid', function (event) {
                        event.preventDefault()
                        if (el.validity.valueMissing) {
                            el.setCustomValidity(this.$t('labelFillInThisField'))
                        }
                        el.reportValidity()
                    })
                },
            },
        },
        data() {
            return {
                crumbsData: [],
                sidebarData: 'DocumentUpload',
                title: 'Form Here',
                maxFiles: 10000000000,
                filesList: [],
                url: null,
                form: {
                    IDEA: '',
                    description: '',
                    emailCreator: '',
                },
                loading: false,
                message: '',
                modalAlertShow: false,
                myInterval: null,
                fileUpload: null,
                chunks: [],
                toastShow: false,
                toastColor: '',
                toastMessage: '',
                timeoutMessage: ENV_CONFIG.VUE_APP_WAITING_TIME_MSG_UPLD,
                timerReq: ENV_CONFIG.VUE_APP_TIMER_REQ,
                dropzoneInstance: null,
            }
        },

        components: {
            NavBar,
            Breadcrumb,
            ModalAlert,
            ToastAlert,
        },
        watch: {
            '$store.state.userProfile.language': function () {
                this.setCrumbsData()
            },
        },
        methods: {
            initializeDropzone() {
                this.dropzoneInstance = new Dropzone('#dropzoneUpload', {
                    paramName: 'file', // Nome do parâmetro de envio
                    maxFilesize: 10000000000, // Tamanho máximo do arquivo em MB
                    acceptedFiles: '.pdf', // Tipos de arquivos aceitos
                    maxFiles: this.maxFiles,
                    autoProcessQueue: false,
                    addRemoveLinks: true,
                })
                this.dropzoneInstance.on('addedfile', this.onFileAdded)
                this.dropzoneInstance.on('removedfile', this.onFileRemove)
            },
            clickUplodFile: function () {
                document.getElementById('inputFileId').click()
            },
            setCrumbsData: function () {
                this.crumbsData = [
                    { crumb: this.$t('labelDocuments'), link: { to: 'DocumentList' } },
                    { crumb: this.$t('labelUpload'), link: { to: 'DocumentUpload' } },
                ]
            },
            onFileAdded(message) {
                if (this.filesList.some((f) => f.name == message.name)) {
                    this.dropzoneInstance.removeFile(message)
                } else {
                    this.filesList.push(message)
                }
                this.updateDropzoneState()
            },
            removeFile(uuid) {
                this.filesList = this.filesList.filter((fileObj) => fileObj.upload.uuid != uuid)
                this.updateDropzoneState()
            },
            removeAllFiles() {
                this.dropzoneInstance.removeAllFiles()
                this.filesList = []
                this.updateDropzoneState()
                this.closeModal()
            },
            updateDropzoneState() {
                const dropzone = this.$refs.dropzone
                if (this.filesList.length > 0) {
                    dropzone.classList.add('files-added')
                } else {
                    dropzone.classList.remove('files-added')
                }
            },
            onFileRemove(message) {
                const fileId = message.upload.uuid
                this.removeFile(fileId)
            },
            checkExceededPages: function () {
                let self = this
                api.get('/Document/CheckExceededPages')
                    .then(function (response) {
                        if (response.data === true) {
                            self.clearMyInterval()
                            self.alertToast(
                                self.$t('labelNumberOfPagesHasBeenExceeded'),
                                'toast-warning'
                            )
                        }
                    })
                    .catch(function (e) {
                        console.log(e)
                    })
                    .finally(function () {
                        console.log('Finished request.')
                    })
            },
            save: function (e) {
                e.preventDefault()
                if (this.filesList.length == 0) {
                    this.clearMyInterval()
                    this.alertToast(this.$t('labelNoFileChosen') + '.', 'toast-warning')
                } else {
                    window.onbeforeunload = function () {
                        return true
                    }
                    this.message = this.$t('labelSendingTheDocument')
                    this.loading = true
                    const apiHeaders = {
                        'X-Email': this.$store.state.userProfile.login,
                        'X-Tenant': this.$store.state.userProfile.tenant,
                        'X-Key-Mongo-Access': this.$store.state.userProfile.keyMongoAccess,
                        'X-Language': this.$store.state.userProfile.language,
                        Authorization: `Bearer ${this.$store.state.userProfile.tokenApi}`,
                    }

                    const chunkSize = 19 * 1024 * 1024
                    const filesNames = this.filesList.map((u) => u.name)
                    const promises = this.filesList.map((fileObj) => {
                        const file = fileObj
                        let additionalData = {
                            name: file.name.replace('.pdf', ''),
                            description: this.form.description,
                            emailCreator: this.$store.state.userProfile.login,
                            filesNames: filesNames,
                        }

                        return this.readFileAsArrayBuffer(file).then((arrayBuffer) => {
                            const totalChunks = Math.ceil(arrayBuffer.byteLength / chunkSize)
                            const chunks = []
                            for (let i = 0; i < totalChunks; i++) {
                                const start = i * chunkSize
                                const end = Math.min(start + chunkSize, arrayBuffer.byteLength)
                                const chunk = arrayBuffer.slice(start, end)
                                chunks.push({
                                    fileChunk: chunk,
                                    fileType: file.type,
                                    additionalData: additionalData,
                                    headers: { ...apiHeaders },
                                    fileName: file.name,
                                    userEmail: this.$store.state.userProfile.login,
                                    tokenAzure: this.$store.state.userProfile.tokenAzure,
                                    url: ENV_CONFIG.VUE_APP_BASE_URL_API,
                                    chunkIndex: i,
                                    totalChunks: totalChunks,
                                })
                            }
                            return chunks
                        })
                    })

                    Promise.all(promises).then((fileDataChunksArray) => {
                        fileDataChunksArray.forEach((chunks) => {
                            chunks.forEach((chunkData) => {
                                uploadFileWorker.send({ message: chunkData })
                            })
                        })
                    })
                    localStorage.setItem('showToast', 'true')
                    this.$router.push({
                        name: 'DocumentList',
                        query: { page: '1', showToast: 'true' },
                    })
                }
            },
            readFileAsArrayBuffer: function (file) {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader()
                    reader.onload = () => resolve(reader.result)
                    reader.onerror = (error) => reject(error)
                    reader.readAsArrayBuffer(file)
                })
            },
            createChunks: function (file) {
                let size = 19922944,
                    chunks = Math.ceil(file.size / size)
                for (let i = 0; i < chunks; i++) {
                    this.chunks.push(
                        file.slice(i * size, Math.min(i * size + size, file.size), file.type)
                    )
                }
            },
            closeModal: function () {
                this.modalAlertShow = false
                document.getElementsByTagName('BODY')[0].children[1].className = 'overlay'
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg
                this.toastColor = color
                this.toastShow = true
                let self = this
                this.myInterval = setInterval(function () {
                    self.toastMessage = ''
                    self.toastColor = ''
                    self.toastShow = false
                    clearInterval(self.myInterval)
                }, 4000)
            },
            closeToast: function () {
                this.toastShow = false
                this.clearMyInterval()
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval)
                this.myInterval = null
            },
            confirmationDialog: function () {
                if (this.filesList.length > 0) {
                    this.modalAlertShow = true
                    document.getElementsByTagName('BODY')[0].children[1].className += ' active'
                } else {
                    this.alertToast(this.$t('labelNoFileChosen') + '.', 'toast-warning')
                }
            },
        },
        computed: {},
        created() {
            this.setCrumbsData()
        },
        mounted() {
            this.initializeDropzone()
        },
        unmounted() {},
    }
</script>

<style scoped>
    .custom-dropzone {
        background-image: url('~@/assets/img/icon-dropzone.svg');
        background-repeat: no-repeat;
        background-position: center;
        color: var(--color-body-content) !important;
        border: 2px dashed #0073e6 !important;
        background-size: 35px;
    }

    .files-added {
        background-image: none !important;
    }

    .label-container {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .clear-button {
        cursor: pointer;
        color: #b1bbcb;
        margin-left: 10px;
        font-weight: bold;
    }

    .input-group-text {
        padding: 0.6rem 0.75rem !important;
    }

    h3 {
        color: black;
        margin-top: 2%;
        text-align: left;
    }

    .form-upload {
        padding-top: 20px !important;
    }

    .custom-file-button input[type='text'],
    .custom-file-button input[type='file'] {
        margin-left: -2px !important;
    }

    .custom-file-button input[type='file']::-webkit-file-upload-button {
        display: none;
    }

    .custom-file-button input[type='file']::file-selector-button {
        display: none;
    }

    .custom-file-button:hover label {
        cursor: pointer;
    }

    .fas {
        font-weight: 900 !important;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .div-center {
        position: relative;
        top: 50%;
        left: 50%;
        -webkit-transform: translate(-50%, -50%);
        transform: translate(-50%, -50%);
        /*width: 500px;*/
    }

    .h5-custom-modal {
        font-weight: initial;
        color: #0073e6;
        text-align: center;
    }

    .border-right {
        border-top-right-radius: 0.25rem !important;
        border-bottom-right-radius: 0.25rem !important;
    }

    /* Refresh animated  */
    .refresh-animated {
        -webkit-animation: spin 2s linear infinite;
        -moz-animation: spin 2s linear infinite;
        animation: spin 2s linear infinite;
    }

    @-moz-keyframes spin {
        100% {
            -moz-transform: rotate(360deg);
        }
    }

    @-webkit-keyframes spin {
        100% {
            -webkit-transform: rotate(360deg);
        }
    }

    @keyframes spin {
        100% {
            -webkit-transform: rotate(360deg);
            transform: rotate(360deg);
        }
    }

    .container-fluid {
        padding: 0 13px;
    }

    #descId {
        height: 100px;
    }

    .red-warning {
        border-color: #dc3545 !important;
    }
    @media (max-width: 767px) {
        .exceedDesc {
            display: none;
        }
    }
</style>

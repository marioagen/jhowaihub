<template>
    <span
        ref="textElement"
        class="truncate-text"
        :class="{ 'truncate-text-icon': isTruncated }"
        v-html="processedText"
        @resize="checkTruncation"
        @click="openModalShow"
    ></span>
    <modal-show v-if="modalShowDesc" @close="closeModal" :dataShow="dataShow"></modal-show>
</template>

<script>
    import ModalShow from '@/components/common/modal-show.vue'

    export default {
        name: 'TruncateText',
        props: {
            item: {
                required: true,
                type: Object,
                default: {},
            },
            text: {
                required: true,
                type: String,
                default: '',
            },
        },
        data() {
            return {
                title: 'Component TruncateText',
                isTruncated: false,
                processedText: '',
                modalShowDesc: false,
                dataShow: {},
            }
        },
        components: {
            ModalShow,
        },
        mounted() {
            this.processText()
            this.checkTruncation()
            window.addEventListener('resize', this.checkTruncation)
        },
        beforeDestroy() {
            window.removeEventListener('resize', this.checkTruncation)
        },
        methods: {
            processText() {
                this.processedText = this.text.replace(/\n/g, '<br />')
            },
            checkTruncation() {
                this.$nextTick(() => {
                    const span = this.$refs.textElement
                    this.isTruncated = false
                    if (span) {
                        this.isTruncated = span.scrollWidth > span.clientWidth
                    }
                })
            },
            openModalShow: function () {
                if (this.isTruncated) {
                    this.modalShowDesc = true
                    this.dataShow = this.item
                    document.getElementsByTagName('BODY')[0].children[1].className += ' active'
                }
            },
            closeModal: function () {
                this.modalShowDesc = false
                document.getElementsByTagName('BODY')[0].children[1].className = 'overlay'
            },
        },
    }
</script>

<style>
    .truncate-text {
        display: inline-block;
        max-width: 100%;
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
        position: relative;
        padding-right: 25px;
    }
    .truncate-text-icon::after {
        content: '\f06e';
        font-family: 'Font Awesome 5 Free';
        font-weight: 900;
        position: absolute;
        right: 5px;
        top: 50%;
        transform: translateY(-50%);
        cursor: pointer;
        pointer-events: auto;
    }
</style>

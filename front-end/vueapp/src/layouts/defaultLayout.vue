<template>
  <div class="app-layout d-flex flex-column h-100" style="height: 100vh;">
    <div class="d-flex flex-grow-1" style="overflow: hidden; position: relative; height: 100%;">
      <div :class="['sidebar-wrapper', { collapsed: isSidebarCollapsed }]">
        <SidebarComponent
          :isCollapsed="isSidebarCollapsed"
          :menuActive="sidebarData"
          @toggle-collapse="toggleSidebar"
        />
      </div>

      <div :class="['content-wrapper', { collapsed: isSidebarCollapsed }]">
        <NavbarComponent 
          :isSidebarCollapsed="isSidebarCollapsed" 
        />
        <router-view :key="$route.fullPath" />
        <toast-notification :showToast="toastShow" @close="closeToast" />
      </div>

      <div
        class="vertical-menu-separator"
        :style="{ left: isSidebarCollapsed ? '60px' : '240px' }"
      ></div>
    </div>

    <div class="horizontal-separator-fixed"></div>
  </div>
</template>

<script>
import GlobalEventService from '@/services/globalEventService'
import SidebarComponent from '@/components/layout/SidebarComponent.vue';
import NavbarComponent from '@/components/layout/NavbarComponent.vue';
import ToastNotification from '@/components/common/toast-notification.vue'

const SIDEBAR_COLLAPSE_WIDTH = 768

export default {
  name: 'DefaultLayout',
  components: {
    NavbarComponent,
    SidebarComponent,
    ToastNotification,
  },
  data() {
    return {
      toastShow: false,
      sidebarData: '',
      isSidebarCollapsed: window.innerWidth < SIDEBAR_COLLAPSE_WIDTH,
    }
  },
  mounted() {
    window.addEventListener('resize', this.checkWindowSize)
    GlobalEventService.on('uploadInProgress', this.handleUploadInProgress)
    GlobalEventService.on('uploadComplete', this.handleUploadComplete)
    GlobalEventService.on('uploadStarted', this.handleUploadStarted)
  },
  beforeUnmount() {
    window.removeEventListener('resize', this.checkWindowSize)
    GlobalEventService.off('uploadInProgress', this.handleUploadInProgress)
    GlobalEventService.off('uploadComplete', this.handleUploadComplete)
    GlobalEventService.off('uploadStarted', this.handleUploadStarted)
  },
  methods: {
    checkWindowSize() {
      this.isSidebarCollapsed = window.innerWidth < SIDEBAR_COLLAPSE_WIDTH
    },
    toggleSidebar() {
      this.isSidebarCollapsed = !this.isSidebarCollapsed
    },
    handleUploadComplete(payload) {},
    handleUploadInProgress(payload) {},
    handleUploadStarted(payload) {
      this.alertToast()
    },
    alertToast(msg, color) {
      this.toastShow = true
    },
    closeToast() {
      this.toastShow = false
    },
  },
}
</script>

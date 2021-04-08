import { mergeStyleSets } from 'office-ui-fabric-react';
import { navCollapsedWidth } from '@m365-admin/nav';
import { getScrollBarWidth } from '@cseo/controls';

const scrollablePaneStyles = {
    position: 'fixed',
    top: 48,
    bottom: 0,
    right: 0
  };
  
  let scrollBarWidth = 0;
  let currentZoomLevel = 0;
  
  const calculcateScrollBarWidth = () => {
    [scrollBarWidth, currentZoomLevel] = getScrollBarWidth(scrollBarWidth, currentZoomLevel, window.devicePixelRatio);
    return scrollBarWidth;
  }
  
  export const _Styles = mergeStyleSets({
    scrollablePaneCollapsed: {
        ...scrollablePaneStyles,
        left: navCollapsedWidth + calculcateScrollBarWidth() + 10
    } ,
    scrollablePaneExpand: {
        ...scrollablePaneStyles,
        left: 228 + calculcateScrollBarWidth() + 10
    },
    rootDiv: {
        paddingRight: '30px',
        paddingLeft: '10px'
    },
    dividerLine: {
        width: '100%',
        height: '1px',
        backgroundColor: 'black',
        marginBottom: '20px'
    },
    rowGap: {
        height: '30px'
    }
  });
  